namespace PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;

public sealed record SpeciesData(
    int Id,
    string Name,
    int Generation,
    string PrimaryType,
    string? SecondaryType);
