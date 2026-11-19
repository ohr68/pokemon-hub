using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Species;

public static class SpeciesErrors
{
    public static Error NotFound(int speciesId)
    {
        return Error.NotFound("Species.NotFound", $"The species with the number {speciesId} was not found.");
    }

    public static readonly Error CatalogUnavailable = Error.Problem(
        "Species.CatalogUnavailable",
        "The species catalog could not be loaded from PokéAPI.");
}
