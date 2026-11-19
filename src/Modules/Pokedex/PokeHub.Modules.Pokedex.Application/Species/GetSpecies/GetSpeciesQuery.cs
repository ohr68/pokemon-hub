using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

/// <param name="Search">Part of the name, or the exact Pokédex number.</param>
public sealed record GetSpeciesQuery(
    string? Search,
    string? Type,
    int? Generation,
    int Page,
    int PageSize) : IQuery<SpeciesPageResponse>;
