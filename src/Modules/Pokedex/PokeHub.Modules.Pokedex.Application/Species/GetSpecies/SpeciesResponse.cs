namespace PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

public sealed record SpeciesResponse(
    int Id,
    string Name,
    int Generation,
    string PrimaryType,
    string? SecondaryType);
