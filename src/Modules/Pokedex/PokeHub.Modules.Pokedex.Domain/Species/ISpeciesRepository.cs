namespace PokeHub.Modules.Pokedex.Domain.Species;

public interface ISpeciesRepository
{
    Task<IReadOnlyList<PokemonSpecies>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PokemonSpecies?> GetAsync(int speciesId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int speciesId, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    void Insert(PokemonSpecies species);

    Task<IReadOnlyList<GrowthRate>> GetGrowthRatesAsync(CancellationToken cancellationToken = default);

    void Insert(GrowthRate growthRate);
}
