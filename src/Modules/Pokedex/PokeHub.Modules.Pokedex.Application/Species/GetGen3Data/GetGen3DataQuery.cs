using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Species.GetGen3Data;

/// <summary>
/// What a Generation III game needs to translate its saves: each species' number in those
/// games, the growth-rate tables to work out levels from experience, and the FireRed/LeafGreen
/// learnsets and moves to give a Pokémon sent into the game its moves. The moves and items are
/// also how a synced Pokémon's page names its moves and held item, in every game.
/// </summary>
public sealed record GetGen3DataQuery : IQuery<Gen3DataResponse>;

public sealed record Gen3DataResponse(
    IReadOnlyCollection<Gen3SpeciesResponse> Species,
    IReadOnlyDictionary<string, int[]> GrowthRates,
    IReadOnlyCollection<Gen3MoveResponse> Moves,
    IReadOnlyCollection<ItemResponse> Items);

/// <param name="LevelUpMoves">In the order the species learns them.</param>
public sealed record Gen3SpeciesResponse(
    int Gen3Index,
    int SpeciesId,
    string Name,
    string? GrowthRate,
    IReadOnlyList<Gen3LevelUpMoveResponse> LevelUpMoves);

public sealed record Gen3LevelUpMoveResponse(int Level, int MoveId);

public sealed record Gen3MoveResponse(int Id, string Name, int Pp);

/// <param name="Gen3Index">Its number in Generation III games, if they have it.</param>
/// <param name="ModernIndex">Its number in X to Ultra Moon, if they have it.</param>
public sealed record ItemResponse(int Id, string Name, int? Gen3Index, int? ModernIndex);
