namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>
/// What a Gen 3 save needs to hold a Pokémon PokeHub creates: each species' number in those
/// games, the experience each level needs, and the FireRed/LeafGreen learnsets and moves.
/// </summary>
/// <param name="ExperienceByLevel">Experience needed for each level (index 0 is level 1), by growth rate.</param>
/// <param name="MovePp">Each move's base PP, by move id.</param>
public sealed record Gen3Catalog(
    IReadOnlyCollection<Gen3SpeciesInfo> Species,
    IReadOnlyDictionary<string, int[]> ExperienceByLevel,
    IReadOnlyDictionary<int, int> MovePp);

/// <param name="Name">PokéAPI's name, such as mr-mime.</param>
/// <param name="LevelUpMoves">In the order the species learns them.</param>
public sealed record Gen3SpeciesInfo(
    int Gen3Index,
    int SpeciesId,
    string Name,
    string? GrowthRate,
    IReadOnlyList<Gen3LevelUpMove> LevelUpMoves);

public sealed record Gen3LevelUpMove(int Level, int MoveId);
