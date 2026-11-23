using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;

public sealed record GameDexData(string GameSlug, IReadOnlyList<int> SpeciesIds);

public interface IPokeApiClient
{
    /// <summary>
    /// Loads every species (generation, types, trade evolution, growth rate, Gen 3 number)
    /// and the growth-rate tables.
    /// </summary>
    Task<Result<SpeciesCatalog>> GetSpeciesCatalogAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads which species each main-series game lists in its own Pokédex, in the game's order.
    /// </summary>
    Task<Result<IReadOnlyCollection<GameDexData>>> GetGameDexesAsync(CancellationToken cancellationToken = default);
}
