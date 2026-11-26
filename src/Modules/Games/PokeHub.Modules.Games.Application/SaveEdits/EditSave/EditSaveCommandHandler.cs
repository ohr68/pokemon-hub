using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveEdits;
using PokeHub.Modules.Games.Domain.SaveFiles;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.SaveEdits.EditSave;

// Mirrors what pokemon-web's apply-pending.ts and box-transfer.ts do in the browser, for a game
// PokeHub can't reach while it runs: settle an edit that never reached the game, back the save
// up, write the waiting Pokémon into free PC slots, record them as delivered and hand the save
// back. Shop items wait for the shop on the 3DS (phase 18f).
internal sealed class EditSaveCommandHandler(
    IGameRepository gameRepository,
    ISaveFileRepository saveFileRepository,
    ISaveEditRepository saveEditRepository,
    IPokedexApi pokedexApi,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<EditSaveCommand, EditedSaveResponse>
{
    private const string Pk3Format = "pk3";
    private const string EditedLabel = "Depois de uma entrega";

    public async Task<Result<EditedSaveResponse>> Handle(EditSaveCommand request, CancellationToken cancellationToken)
    {
        Result<Game> resolved = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);
        if (resolved.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(resolved.Error);
        }

        Game game = resolved.Value;
        var gen3Game = Gen3Game.ForSlug(game.Slug);
        if (gen3Game is null)
        {
            return Result.Failure<EditedSaveResponse>(SaveEditErrors.NotEditable);
        }

        Gen3Save save;
        try
        {
            save = Gen3Save.Read(request.Content, gen3Game.Family);
        }
        catch (Gen3SaveException exception)
        {
            return Result.Failure<EditedSaveResponse>(SaveEditErrors.Unreadable(exception.Message));
        }

        if (save.Layout.Family != gen3Game.Family)
        {
            return Result.Failure<EditedSaveResponse>(SaveEditErrors.Unreadable($"This save isn't from a {gen3Game.Family} game."));
        }

        var present = save.Pokemon().Select(p => p.CatchKey).ToHashSet(StringComparer.Ordinal);

        Result<int> settled = await SettleOpenEditsAsync(request.TrainerId, game.Id, present, cancellationToken);
        if (settled.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(settled.Error);
        }

        await BackUpAsync(request.TrainerId, game.Id, request.Content, request.Label, cancellationToken);

        Result<IReadOnlyCollection<DeliveryInfo>> awaiting =
            await pokedexApi.GetAwaitingDeliveryAsync(request.TrainerId, game.Id, cancellationToken);
        if (awaiting.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(awaiting.Error);
        }

        if (awaiting.Value.Count == 0)
        {
            await SaveAsync(request.TrainerId, game.Id, cancellationToken);
            return new EditedSaveResponse(request.Content, false, [], [], settled.Value);
        }

        (List<(DeliveryInfo Pokemon, string CatchKey, int Slot)> delivered, List<NotDeliveredResponse> notDelivered) =
            await WritePokemonAsync(save, gen3Game, awaiting.Value, game.Name, cancellationToken);

        if (delivered.Count == 0)
        {
            await SaveAsync(request.TrainerId, game.Id, cancellationToken);
            return new EditedSaveResponse(request.Content, false, [], notDelivered, settled.Value);
        }

        // Recorded before the save is handed out; the journal settles it if the device never
        // writes it. All are recorded, or none.
        List<(Guid PokemonId, string CatchKey)> deliveries = [.. delivered.Select(d => (d.Pokemon.Id, d.CatchKey))];
        Result recorded = await pokedexApi.DeliverPokemonAsync(
            request.TrainerId,
            game.Id,
            [.. deliveries.Select(d => new DeliveredPokemon(d.PokemonId, d.CatchKey))],
            cancellationToken);
        if (recorded.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(recorded.Error);
        }

        byte[] edited = save.ToArray();
        saveEditRepository.Insert(SaveEdit.Create(request.TrainerId, game.Id, deliveries, dateTimeProvider.UtcNow));

        // The edited save is the latest backup, so the game's page syncs it rather than the one
        // without the delivered Pokémon (which would read as them having left the game).
        await BackUpAsync(request.TrainerId, game.Id, edited, EditedLabel, cancellationToken);
        await SaveAsync(request.TrainerId, game.Id, cancellationToken);

        return new EditedSaveResponse(
            edited,
            true,
            [.. delivered.Select(d =>
            {
                (int box, int slot) = Gen3Save.BoxPosition(d.Slot);
                return new DeliveredToSaveResponse(d.Pokemon.Id, NameOf(d.Pokemon), box, slot);
            })],
            notDelivered,
            settled.Value);
    }

    // Pokémon of earlier edits missing from this save never reached the game: they wait for
    // delivery again. Ones PokeHub lists as waiting already were never recorded as delivered.
    private async Task<Result<int>> SettleOpenEditsAsync(
        Guid trainerId,
        Guid gameId,
        HashSet<string> present,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SaveEdit> open = await saveEditRepository.GetOpenAsync(trainerId, gameId, cancellationToken);
        if (open.Count == 0)
        {
            return 0;
        }

        var missing = open.SelectMany(edit => edit.MissingFrom(present)).ToHashSet();
        int redelivered = 0;

        if (missing.Count > 0)
        {
            Result<IReadOnlyCollection<DeliveryInfo>> waiting =
                await pokedexApi.GetAwaitingDeliveryAsync(trainerId, gameId, cancellationToken);
            if (waiting.IsFailure)
            {
                return Result.Failure<int>(waiting.Error);
            }

            missing.ExceptWith(waiting.Value.Select(p => p.Id));
            if (missing.Count > 0)
            {
                Result result = await pokedexApi.RedeliverAsync(trainerId, gameId, [.. missing], [.. present], cancellationToken);
                if (result.IsFailure)
                {
                    return Result.Failure<int>(result.Error);
                }

                redelivered = missing.Count;
            }
        }

        foreach (SaveEdit edit in open)
        {
            edit.Settle(dateTimeProvider.UtcNow);
        }

        return redelivered;
    }

    private async Task<(List<(DeliveryInfo, string, int)>, List<NotDeliveredResponse>)> WritePokemonAsync(
        Gen3Save save,
        Gen3Game gen3Game,
        IReadOnlyCollection<DeliveryInfo> awaiting,
        string gameName,
        CancellationToken cancellationToken)
    {
        Gen3Catalog catalog = await pokedexApi.GetGen3CatalogAsync(cancellationToken);
        var speciesById = catalog.Species.ToDictionary(s => s.SpeciesId);
        var free = new Queue<int>(save.FreeSlots());

        var delivered = new List<(DeliveryInfo, string, int)>();
        var notDelivered = new List<NotDeliveredResponse>();

        foreach (DeliveryInfo pokemon in awaiting)
        {
            if (!speciesById.TryGetValue(pokemon.SpeciesId, out Gen3SpeciesInfo? species))
            {
                notDelivered.Add(new NotDeliveredResponse(pokemon.Id, NameOf(pokemon), $"{gameName} doesn't have this species."));
                continue;
            }

            if (!free.TryDequeue(out int slot))
            {
                notDelivered.Add(new NotDeliveredResponse(pokemon.Id, NameOf(pokemon), "There's no free slot left in the PC boxes."));
                continue;
            }

            byte[] bytes = StoredPk3(pokemon) ?? Gen3NewPokemon.Create(
                new HubPokemon(
                    pokemon.Nickname,
                    pokemon.Level,
                    pokemon.IsShiny,
                    pokemon.IsOriginalTrainer,
                    pokemon.OriginalTrainerId.ToString(),
                    pokemon.OriginalTrainerName),
                new Gen3Species(
                    species.Gen3Index,
                    species.Name,
                    species.GrowthRate is not null && catalog.ExperienceByLevel.TryGetValue(species.GrowthRate, out int[]? table) ? table : [],
                    [.. species.LevelUpMoves.Select(m => (m.Level, m.MoveId))]),
                save,
                gen3Game,
                catalog.MovePp);

            save.SetSlot(slot, bytes);
            Gen3Pokemon written = Gen3Save.Read(save.ToArray(), gen3Game.Family).Pokemon().First(p => p.BoxSlot == slot);
            delivered.Add((pokemon, written.CatchKey, slot));
        }

        return (delivered, notDelivered);
    }

    // A Pokémon that was in a Gen 3 game before comes back exactly as it left.
    private static byte[]? StoredPk3(DeliveryInfo pokemon) =>
        pokemon is { GameDataFormat: Pk3Format, GameData.Length: Pk3.Size } ? pokemon.GameData : null;

    // As the game shows it: its nickname, or its species in capitals.
    private static string NameOf(DeliveryInfo pokemon) => pokemon.Nickname ?? Gen3NewPokemon.DisplayName(pokemon.SpeciesName);

    private async Task SaveAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken)
    {
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await saveFileRepository.RemoveOldestAsync(trainerId, gameId, SaveFile.BackupsKept, cancellationToken);
    }

    private async Task BackUpAsync(Guid trainerId, Guid gameId, byte[] content, string? label, CancellationToken cancellationToken)
    {
        // An unchanged save isn't stored twice.
        SaveFile? latest = await saveFileRepository.GetLatestAsync(trainerId, gameId, cancellationToken);
        if (latest is not null && latest.Sha256 == SaveFile.HashOf(content))
        {
            return;
        }

        saveFileRepository.Insert(SaveFile.Upload(trainerId, gameId, content, dateTimeProvider.UtcNow, label));
    }
}
