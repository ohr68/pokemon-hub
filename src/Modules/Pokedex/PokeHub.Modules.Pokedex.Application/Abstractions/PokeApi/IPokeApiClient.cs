using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;

public interface IPokeApiClient
{
    /// <summary>
    /// Loads every species with its generation and the types of its default form.
    /// </summary>
    Task<Result<IReadOnlyCollection<SpeciesData>>> GetSpeciesCatalogAsync(CancellationToken cancellationToken = default);
}
