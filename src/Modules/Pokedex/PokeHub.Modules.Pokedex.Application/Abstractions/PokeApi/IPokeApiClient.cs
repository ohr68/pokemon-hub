using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;

public interface IPokeApiClient
{
    /// <summary>
    /// Loads every species (generation, types, trade evolution, growth rate, Gen 3 number)
    /// and the growth-rate tables.
    /// </summary>
    Task<Result<SpeciesCatalog>> GetSpeciesCatalogAsync(CancellationToken cancellationToken = default);
}
