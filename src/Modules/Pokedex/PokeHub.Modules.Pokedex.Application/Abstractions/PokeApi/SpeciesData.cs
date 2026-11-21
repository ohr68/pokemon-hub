namespace PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;

/// <param name="TradeEvolutionSpeciesId">The species it becomes when traded without a held item, if any.</param>
/// <param name="GrowthRate">The name of its growth rate.</param>
/// <param name="Gen3Index">Its number inside Generation III games, for species 1 to 386.</param>
/// <param name="Gen3LevelUpMoves">The moves it learns by level in FireRed and LeafGreen, in the game's order.</param>
public sealed record SpeciesData(
    int Id,
    string Name,
    int Generation,
    string PrimaryType,
    string? SecondaryType,
    int? TradeEvolutionSpeciesId,
    string? GrowthRate,
    int? Gen3Index,
    IReadOnlyList<LevelUpMoveData> Gen3LevelUpMoves,
    bool IsLegendary,
    bool IsMythical);

public sealed record LevelUpMoveData(int Level, int MoveId);

public sealed record MoveData(int Id, string Name, int Pp);

/// <param name="ExperienceByLevel">Total experience for levels 1 to 100.</param>
public sealed record GrowthRateData(string Name, int[] ExperienceByLevel);

public sealed record SpeciesCatalog(
    IReadOnlyCollection<SpeciesData> Species,
    IReadOnlyCollection<GrowthRateData> GrowthRates,
    IReadOnlyCollection<MoveData> Moves);
