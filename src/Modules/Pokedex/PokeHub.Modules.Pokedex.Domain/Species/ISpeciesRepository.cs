namespace PokeHub.Modules.Pokedex.Domain.Species;

public interface ISpeciesRepository
{
    Task<IReadOnlyList<PokemonSpecies>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PokemonSpecies?> GetAsync(int speciesId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int speciesId, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<bool> HasEvolutionLinesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The stages from one species up to a later form of it, without the first (Charmander to
    /// Charizard is Charmeleon, Charizard). Empty when the second isn't a later form.
    /// </summary>
    Task<IReadOnlyList<int>> GetEvolutionPathAsync(int fromSpeciesId, int toSpeciesId, CancellationToken cancellationToken = default);

    void Insert(PokemonSpecies species);

    Task<IReadOnlyList<GrowthRate>> GetGrowthRatesAsync(CancellationToken cancellationToken = default);

    void Insert(GrowthRate growthRate);

    Task<IReadOnlyList<Move>> GetMovesAsync(CancellationToken cancellationToken = default);

    void Insert(Move move);

    Task<IReadOnlyList<Item>> GetItemsAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyItemsAsync(CancellationToken cancellationToken = default);

    void Insert(Item item);

    Task<bool> AnyGameDexEntriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<int>> GetIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>Replaces every game's Pokédex list; saved with the unit of work.</summary>
    Task ReplaceGameDexEntriesAsync(IReadOnlyCollection<GameDexEntry> entries, CancellationToken cancellationToken = default);
}
