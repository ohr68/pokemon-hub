namespace PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

public sealed record SpeciesPageResponse(
    IReadOnlyCollection<SpeciesResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
