using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Species.SyncSpecies;

internal sealed class SyncSpeciesCommandHandler(
    IPokeApiClient pokeApiClient,
    ISpeciesRepository speciesRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SyncSpeciesCommand, SpeciesSyncResponse>
{
    public async Task<Result<SpeciesSyncResponse>> Handle(SyncSpeciesCommand request, CancellationToken cancellationToken)
    {
        // Evolution lines and items arrived after the catalog: an existing one needs one full sync for them.
        if (request.OnlyIfEmpty
            && await speciesRepository.AnyAsync(cancellationToken)
            && await speciesRepository.HasEvolutionLinesAsync(cancellationToken)
            && await speciesRepository.AnyItemsAsync(cancellationToken))
        {
            // Game lists arrived after the catalog: an existing catalog still needs them once.
            if (!await speciesRepository.AnyGameDexEntriesAsync(cancellationToken))
            {
                await SyncGameDexesAsync(cancellationToken);
            }

            return new SpeciesSyncResponse(0, 0, 0, Skipped: true);
        }

        Result<SpeciesCatalog> catalog = await pokeApiClient.GetSpeciesCatalogAsync(cancellationToken);

        if (catalog.IsFailure)
        {
            return Result.Failure<SpeciesSyncResponse>(catalog.Error);
        }

        var existing = (await speciesRepository.GetAllAsync(cancellationToken))
            .ToDictionary(s => s.Id);

        DateTime utcNow = dateTimeProvider.UtcNow;
        int added = 0;
        int updated = 0;

        await SyncGrowthRatesAsync(catalog.Value.GrowthRates, cancellationToken);
        await SyncMovesAsync(catalog.Value.Moves, cancellationToken);
        await SyncItemsAsync(catalog.Value.Items, cancellationToken);

        foreach (SpeciesData data in catalog.Value.Species)
        {
            if (existing.TryGetValue(data.Id, out PokemonSpecies? species))
            {
                if (species.Update(
                        data.Name,
                        data.Generation,
                        data.PrimaryType,
                        data.SecondaryType,
                        data.TradeEvolutionSpeciesId,
                        data.GrowthRate,
                        data.Gen3Index,
                        ToLevelUpMoves(data.Gen3LevelUpMoves),
                        data.IsLegendary,
                        data.IsMythical,
                        data.EvolvesFromSpeciesId,
                        utcNow))
                {
                    updated++;
                }

                continue;
            }

            speciesRepository.Insert(PokemonSpecies.Create(
                data.Id,
                data.Name,
                data.Generation,
                data.PrimaryType,
                data.SecondaryType,
                data.TradeEvolutionSpeciesId,
                data.GrowthRate,
                data.Gen3Index,
                ToLevelUpMoves(data.Gen3LevelUpMoves),
                data.IsLegendary,
                data.IsMythical,
                data.EvolvesFromSpeciesId,
                utcNow));

            added++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await SyncGameDexesAsync(cancellationToken);

        return new SpeciesSyncResponse(added, updated, catalog.Value.Species.Count);
    }

    // Extra to the catalog: if PokéAPI fails here, the current lists stay and the catalog still counts.
    private async Task SyncGameDexesAsync(CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<GameDexData>> dexes = await pokeApiClient.GetGameDexesAsync(cancellationToken);

        if (dexes.IsFailure)
        {
            return;
        }

        var known = (await speciesRepository.GetIdsAsync(cancellationToken)).ToHashSet();

        var entries = dexes.Value
            .SelectMany(dex => dex.SpeciesIds
                .Where(known.Contains)
                .Select((speciesId, position) => GameDexEntry.Create(dex.GameSlug, speciesId, position)))
            .ToList();

        await speciesRepository.ReplaceGameDexEntriesAsync(entries, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncGrowthRatesAsync(IReadOnlyCollection<GrowthRateData> growthRates, CancellationToken cancellationToken)
    {
        var existing = (await speciesRepository.GetGrowthRatesAsync(cancellationToken)).ToDictionary(r => r.Name);

        foreach (GrowthRateData data in growthRates)
        {
            if (existing.TryGetValue(data.Name, out GrowthRate? growthRate))
            {
                growthRate.Update(data.ExperienceByLevel);
                continue;
            }

            speciesRepository.Insert(GrowthRate.Create(data.Name, data.ExperienceByLevel));
        }
    }

    private async Task SyncMovesAsync(IReadOnlyCollection<MoveData> moves, CancellationToken cancellationToken)
    {
        var existing = (await speciesRepository.GetMovesAsync(cancellationToken)).ToDictionary(m => m.Id);

        foreach (MoveData data in moves)
        {
            if (existing.TryGetValue(data.Id, out Move? move))
            {
                move.Update(data.Name, data.Pp);
                continue;
            }

            speciesRepository.Insert(Move.Create(data.Id, data.Name, data.Pp));
        }
    }

    private async Task SyncItemsAsync(IReadOnlyCollection<ItemData> items, CancellationToken cancellationToken)
    {
        var existing = (await speciesRepository.GetItemsAsync(cancellationToken)).ToDictionary(i => i.Id);

        foreach (ItemData data in items)
        {
            if (existing.TryGetValue(data.Id, out Item? item))
            {
                item.Update(data.Name, data.Gen3Index, data.ModernIndex);
                continue;
            }

            speciesRepository.Insert(Item.Create(data.Id, data.Name, data.Gen3Index, data.ModernIndex));
        }
    }

    private static List<LevelUpMove> ToLevelUpMoves(IReadOnlyList<LevelUpMoveData> moves)
    {
        return moves.Select((m, index) => new LevelUpMove(index, m.Level, m.MoveId)).ToList();
    }
}
