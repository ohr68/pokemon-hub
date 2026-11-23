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

    Task<IReadOnlyList<Move>> GetMovesAsync(CancellationToken cancellationToken = default);

    void Insert(Move move);

    Task<bool> AnyGameDexEntriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>Replaces every game's Pokédex list; saved with the unit of work.</summary>
    Task ReplaceGameDexEntriesAsync(IReadOnlyCollection<GameDexEntry> entries, CancellationToken cancellationToken = default);
}
