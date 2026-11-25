using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Species.GetGrowthData;

/// <summary>
/// What a game whose saves store national numbers (Generation VI onwards) needs to read them:
/// every species' name and growth rate, and the growth-rate tables, to work out a boxed
/// Pokémon's level from its experience.
/// </summary>
public sealed record GetGrowthDataQuery : IQuery<GrowthDataResponse>;

public sealed record GrowthDataResponse(
    IReadOnlyCollection<GrowthSpeciesResponse> Species,
    IReadOnlyDictionary<string, int[]> GrowthRates);

public sealed record GrowthSpeciesResponse(int SpeciesId, string Name, string? GrowthRate);
