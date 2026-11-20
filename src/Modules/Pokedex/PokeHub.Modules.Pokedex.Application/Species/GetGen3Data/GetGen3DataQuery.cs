using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Species.GetGen3Data;

/// <summary>
/// What a Generation III game needs to translate its saves: each species' number in those
/// games, the growth-rate tables to work out levels from experience, and the FireRed/LeafGreen
/// learnsets and moves to give a Pokémon sent into the game its moves.
/// </summary>
public sealed record GetGen3DataQuery : IQuery<Gen3DataResponse>;

public sealed record Gen3DataResponse(
    IReadOnlyCollection<Gen3SpeciesResponse> Species,
    IReadOnlyDictionary<string, int[]> GrowthRates,
    IReadOnlyCollection<Gen3MoveResponse> Moves);

/// <param name="LevelUpMoves">In the order the species learns them.</param>
public sealed record Gen3SpeciesResponse(
    int Gen3Index,
    int SpeciesId,
    string Name,
    string? GrowthRate,
    IReadOnlyList<Gen3LevelUpMoveResponse> LevelUpMoves);

public sealed record Gen3LevelUpMoveResponse(int Level, int MoveId);

public sealed record Gen3MoveResponse(int Id, string Name, int Pp);
