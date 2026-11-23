namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>A catch or evolution that happened inside a game.</summary>
/// <param name="IsEvolution">It evolved in the game; otherwise it was caught there.</param>
/// <param name="SpeciesId">The species it was caught as, or evolved into.</param>
public sealed record AchievementFact(Guid GameId, bool IsEvolution, Guid PokemonId, int SpeciesId, bool IsShiny);

/// <summary>What a trainer has done inside one game.</summary>
/// <param name="SpeciesRegistered">Different species caught in this game or evolved into in it.</param>
public sealed record GameProgress(
    Guid GameId,
    int Catches,
    int Evolutions,
    int ShinyCatches,
    int SpeciesRegistered,
    int Badges)
{
    public int Value(GameStat stat) => stat switch
    {
        GameStat.Catches => Catches,
        GameStat.Evolutions => Evolutions,
        GameStat.ShinyCatches => ShinyCatches,
        GameStat.SpeciesRegistered => SpeciesRegistered,
        GameStat.Badges => Badges,
        _ => 0,
    };
}

/// <summary>What connects a trainer's games.</summary>
/// <param name="GamesPlayed">Games with at least one catch, evolution or badge.</param>
/// <param name="SameSpeciesCaught">The most games any one species was caught in.</param>
/// <param name="SameSpeciesEvolved">The most games any one species was evolved into in.</param>
public sealed record CrossGameProgress(int GamesPlayed, int SameSpeciesCaught, int SameSpeciesEvolved)
{
    public int Value(CrossGameStat stat) => stat switch
    {
        CrossGameStat.GamesPlayed => GamesPlayed,
        CrossGameStat.SameSpeciesCaught => SameSpeciesCaught,
        CrossGameStat.SameSpeciesEvolved => SameSpeciesEvolved,
        _ => 0,
    };
}

/// <summary>
/// A trainer's progress per game and across games, worked out from what happened rather than
/// stored, so past catches count and a new kind of goal needs no data migration.
/// </summary>
public sealed record AchievementProgress(IReadOnlyList<GameProgress> Games, CrossGameProgress CrossGame)
{
    /// <param name="facts">Every catch and evolution in a game.</param>
    /// <param name="badgeGameIds">The game of each badge earned, one entry per badge.</param>
    public static AchievementProgress From(IReadOnlyCollection<AchievementFact> facts, IReadOnlyCollection<Guid> badgeGameIds)
    {
        var games = facts
            .Select(f => f.GameId)
            .Concat(badgeGameIds)
            .Distinct()
            .Order()
            .Select(gameId => ForGame(gameId, facts, badgeGameIds))
            .ToList();

        return new AchievementProgress(
            games,
            new CrossGameProgress(
                games.Count,
                MostGames(facts.Where(f => !f.IsEvolution)),
                MostGames(facts.Where(f => f.IsEvolution))));
    }

    private static GameProgress ForGame(Guid gameId, IReadOnlyCollection<AchievementFact> facts, IReadOnlyCollection<Guid> badgeGameIds)
    {
        var inGame = facts.Where(f => f.GameId == gameId).ToList();
        var caught = inGame.Where(f => !f.IsEvolution).DistinctBy(f => f.PokemonId).ToList();
        var evolved = inGame.Where(f => f.IsEvolution).DistinctBy(f => (f.PokemonId, f.SpeciesId)).ToList();

        return new GameProgress(
            gameId,
            caught.Count,
            evolved.Count,
            caught.Count(f => f.IsShiny),
            caught.Concat(evolved).Select(f => f.SpeciesId).Distinct().Count(),
            badgeGameIds.Count(id => id == gameId));
    }

    private static int MostGames(IEnumerable<AchievementFact> facts) =>
        facts
            .GroupBy(f => f.SpeciesId)
            .Select(species => species.Select(f => f.GameId).Distinct().Count())
            .DefaultIfEmpty()
            .Max();
}
