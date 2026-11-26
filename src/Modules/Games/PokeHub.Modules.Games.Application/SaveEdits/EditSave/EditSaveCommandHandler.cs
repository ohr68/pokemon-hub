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
// up, take the chosen Pokémon out to the hub, write the waiting ones into free PC slots, and
// hand the save back. Shop items wait for the shop on the 3DS (phase 18f).
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
    private const string FromOtherGameCode = "Pokemon.FromOtherGame";

    public async Task<Result<EditedSaveResponse>> Handle(EditSaveCommand request, CancellationToken cancellationToken)
    {
        Result<Game> resolved = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);
        if (resolved.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(resolved.Error);
        }

        Game game = resolved.Value;
        Result<(Gen3Save, Gen3Game)> read = SaveEditReader.Read(game, request.Content);
        if (read.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(read.Error);
        }

        (Gen3Save save, Gen3Game gen3Game) = read.Value;
        var present = save.Pokemon().Select(p => p.CatchKey).ToHashSet(StringComparer.Ordinal);

        Result<int> settled = await SettleOpenEditsAsync(request.TrainerId, game.Id, present, cancellationToken);
        if (settled.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(settled.Error);
        }

        await BackUpAsync(request.TrainerId, game.Id, request.Content, request.Label, cancellationToken);

        Gen3Catalog catalog = await pokedexApi.GetGen3CatalogAsync(cancellationToken);

        Result<List<WithdrawnFromSaveResponse>> withdrawn = await WithdrawAsync(
            request, game, save, gen3Game, catalog, cancellationToken);
        if (withdrawn.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(withdrawn.Error);
        }

        Result<IReadOnlyCollection<DeliveryInfo>> awaiting =
            await pokedexApi.GetAwaitingDeliveryAsync(request.TrainerId, game.Id, cancellationToken);
        if (awaiting.IsFailure)
        {
            return Result.Failure<EditedSaveResponse>(awaiting.Error);
        }

        (List<(DeliveryInfo Pokemon, string CatchKey, int Slot)> delivered, List<NotDeliveredResponse> notDelivered) =
            WritePokemon(save, gen3Game, catalog, awaiting.Value, game.Name);

        // Recorded before the save is handed out; the journal settles it if the device never
        // writes it. All are recorded, or none.
        List<(Guid PokemonId, string CatchKey)> deliveries = [.. delivered.Select(d => (d.Pokemon.Id, d.CatchKey))];
        if (deliveries.Count > 0)
        {
            Result recorded = await pokedexApi.DeliverPokemonAsync(
                request.TrainerId,
                game.Id,
                [.. deliveries.Select(d => new DeliveredPokemon(d.PokemonId, d.CatchKey))],
                cancellationToken);
            if (recorded.IsFailure)
            {
                return Result.Failure<EditedSaveResponse>(recorded.Error);
            }
        }

        if (deliveries.Count == 0 && withdrawn.Value.Count == 0)
        {
            await SaveAsync(request.TrainerId, game.Id, cancellationToken);
            return new EditedSaveResponse(request.Content, false, [], notDelivered, [], settled.Value);
        }

        byte[] edited = save.ToArray();
        saveEditRepository.Insert(SaveEdit.Create(
            request.TrainerId,
            game.Id,
            deliveries,
            [.. request.Withdraw ?? []],
            dateTimeProvider.UtcNow));

        // The edited save is the latest backup, so the game's page syncs it rather than the one
        // before the edit (which would read as delivered Pokémon having left the game).
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
            withdrawn.Value,
            settled.Value);
    }

    // An earlier edit the game never got shows in this save: delivered Pokémon missing from it
    // wait for delivery again, and withdrawn ones still in it go back inside the game.
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

        int fixedCount = 0;
        var missing = open.SelectMany(edit => edit.MissingFrom(present)).ToHashSet();

        if (missing.Count > 0)
        {
            // Ones PokeHub lists as waiting already were never recorded as delivered.
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

                fixedCount += missing.Count;
            }
        }

        var stillThere = open.SelectMany(edit => edit.StillIn(present)).ToHashSet(StringComparer.Ordinal);
        if (stillThere.Count > 0)
        {
            Result result = await pokedexApi.CancelWithdrawalAsync(trainerId, gameId, [.. stillThere], [.. present], cancellationToken);
            if (result.IsFailure)
            {
                return Result.Failure<int>(result.Error);
            }

            fixedCount += stillThere.Count;
        }

        foreach (SaveEdit edit in open)
        {
            edit.Settle(dateTimeProvider.UtcNow);
        }

        return fixedCount;
    }

    // Like pokemon-web's withdrawFromGame: PokeHub must know each Pokémon before it can leave the
    // game, so its catch is recorded again first (harmless for a known one). One from another
    // game that PokeHub never moved here can't leave through this game. They move with their
    // exact bytes, so they come back identical.
    private async Task<Result<List<WithdrawnFromSaveResponse>>> WithdrawAsync(
        EditSaveCommand request,
        Game game,
        Gen3Save save,
        Gen3Game gen3Game,
        Gen3Catalog catalog,
        CancellationToken cancellationToken)
    {
        if (request.Withdraw is not { Count: > 0 })
        {
            return new List<WithdrawnFromSaveResponse>();
        }

        var inBoxes = save.Pokemon()
            .Where(p => p.Location == Gen3Location.Box)
            .ToDictionary(p => p.CatchKey, StringComparer.Ordinal);
        var speciesByIndex = catalog.Species.ToDictionary(s => s.Gen3Index);
        var chosen = new List<(Gen3Pokemon Pokemon, Gen3SpeciesInfo Species, int Level, string? Nickname, Guid Id)>();

        foreach (string catchKey in request.Withdraw.Distinct(StringComparer.Ordinal))
        {
            if (!inBoxes.TryGetValue(catchKey, out Gen3Pokemon? pokemon) ||
                !speciesByIndex.TryGetValue(pokemon.Gen3Species, out Gen3SpeciesInfo? species))
            {
                return Result.Failure<List<WithdrawnFromSaveResponse>>(SaveEditErrors.NotInBoxes);
            }

            int level = Gen3NewPokemon.LevelFromExperience(
                pokemon.Experience,
                species.GrowthRate is not null && catalog.ExperienceByLevel.TryGetValue(species.GrowthRate, out int[]? table) ? table : []);
            string? nickname = Gen3NewPokemon.RealNickname(pokemon.Nickname, species.Name);

            Result<Guid> caught = await pokedexApi.CatchPokemonAsync(
                new CatchRequest(
                    request.TrainerId,
                    game.Id,
                    game.Name,
                    species.SpeciesId,
                    level,
                    pokemon.IsShiny,
                    catchKey,
                    nickname,
                    FromOtherGame: pokemon.OriginGame != gen3Game.OriginGame),
                cancellationToken);
            if (caught.IsFailure)
            {
                string name = nickname ?? Gen3NewPokemon.DisplayName(species.Name);
                return Result.Failure<List<WithdrawnFromSaveResponse>>(caught.Error.Code == FromOtherGameCode
                    ? SaveEditErrors.FromOtherGame(name, game.Name)
                    : caught.Error);
            }

            chosen.Add((pokemon, species, level, nickname, caught.Value));
        }

        Result<IReadOnlyCollection<Guid>> moved = await pokedexApi.WithdrawPokemonAsync(
            request.TrainerId,
            game.Id,
            [.. chosen.Select(c => new WithdrawnPokemon(c.Pokemon.CatchKey, c.Species.SpeciesId, c.Level, c.Nickname, Pk3Format, c.Pokemon.Raw.ToArray()))],
            cancellationToken);
        if (moved.IsFailure)
        {
            return Result.Failure<List<WithdrawnFromSaveResponse>>(moved.Error);
        }

        foreach ((Gen3Pokemon pokemon, _, _, _, _) in chosen)
        {
            save.SetSlot(pokemon.BoxSlot!.Value, []);
        }

        return chosen.Select(c => new WithdrawnFromSaveResponse(c.Id, c.Nickname ?? Gen3NewPokemon.DisplayName(c.Species.Name))).ToList();
    }

    private static (List<(DeliveryInfo, string, int)>, List<NotDeliveredResponse>) WritePokemon(
        Gen3Save save,
        Gen3Game gen3Game,
        Gen3Catalog catalog,
        IReadOnlyCollection<DeliveryInfo> awaiting,
        string gameName)
    {
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
