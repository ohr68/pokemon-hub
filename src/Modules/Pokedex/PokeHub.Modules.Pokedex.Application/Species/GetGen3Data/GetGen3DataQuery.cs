using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Species.GetGen3Data;

/// <summary>
/// What a Generation III game needs to translate its saves: each species' number in those
/// games, and the growth-rate tables to work out levels from experience.
/// </summary>
public sealed record GetGen3DataQuery : IQuery<Gen3DataResponse>;

public sealed record Gen3DataResponse(
    IReadOnlyCollection<Gen3SpeciesResponse> Species,
    IReadOnlyDictionary<string, int[]> GrowthRates);

public sealed record Gen3SpeciesResponse(int Gen3Index, int SpeciesId, string Name, string? GrowthRate);
