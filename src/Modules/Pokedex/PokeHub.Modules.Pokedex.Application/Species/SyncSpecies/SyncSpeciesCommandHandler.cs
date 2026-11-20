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
        if (request.OnlyIfEmpty && await speciesRepository.AnyAsync(cancellationToken))
        {
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
                utcNow));

            added++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SpeciesSyncResponse(added, updated, catalog.Value.Species.Count);
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
}
