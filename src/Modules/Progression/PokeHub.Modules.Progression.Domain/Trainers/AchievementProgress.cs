namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>A catch or evolution that happened inside a game.</summary>
/// <param name="IsEvolution">It evolved in the game; otherwise it was caught there.</param>
/// <param name="SpeciesId">The species it was caught as, or evolved into.</param>
public sealed record AchievementFact(Guid GameId, bool IsEvolution, Guid PokemonId, int SpeciesId, bool IsShiny);

/// <summary>What a game offers a trainer, beyond what they did in it.</summary>
/// <param name="DexRegistered">Species of the game's own Pokédex registered in their save.</param>
/// <param name="DexTotal">Species in that Pokédex; zero when the game has no known list.</param>
/// <param name="TotalBadges">Badges in the game; zero until it reports them.</param>
public sealed record GameContext(Guid GameId, int DexRegistered, int DexTotal, int TotalBadges);

/// <summary>What a trainer has done inside one game.</summary>
/// <param name="SpeciesRegistered">Different species caught in this game or evolved into in it.</param>
public sealed record GameProgress(
    Guid GameId,
    int Catches,
    int Evolutions,
    int ShinyCatches,
    int SpeciesRegistered,
    int Badges,
    int DexRegistered = 0,
    int DexTotal = 0,
    int TotalBadges = 0)
{
    public int DexPercent => DexTotal == 0 ? 0 : 100 * DexRegistered / DexTotal;

    public int BadgePercent => TotalBadges == 0 ? 0 : 100 * Badges / TotalBadges;

    /// <summary>Whether this game can offer the trophy: one with no Pokédex or badges can't offer those.</summary>
    public bool Offers(GameTrophy trophy) => trophy.Stat switch
    {
        GameStat.DexPercent => DexTotal > 0,
        GameStat.Badges or GameStat.BadgePercent => TotalBadges > 0,
        _ => true,
    };

    /// <summary>Progress toward the trophy in things to do: 38 of 76 species rather than 50%.</summary>
    public (int Current, int Target) Measure(GameTrophy trophy) => trophy.Stat switch
    {
        GameStat.DexPercent => (DexRegistered, PartOf(DexTotal, trophy.Target)),
        GameStat.BadgePercent => (Badges, PartOf(TotalBadges, trophy.Target)),
        _ => (Value(trophy.Stat), trophy.Target),
    };

    // Rounded up, so reaching it always means reaching the percentage.
    private static int PartOf(int total, int percent) => (total * percent + 99) / 100;

    public int Value(GameStat stat) => stat switch
    {
        GameStat.Catches => Catches,
        GameStat.Evolutions => Evolutions,
        GameStat.ShinyCatches => ShinyCatches,
        GameStat.SpeciesRegistered => SpeciesRegistered,
        GameStat.Badges => Badges,
        GameStat.DexPercent => DexPercent,
        GameStat.BadgePercent => BadgePercent,
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
    /// <param name="contexts">What each game offers; a game left out offers no Pokédex or badge goals.</param>
    public static AchievementProgress From(
        IReadOnlyCollection<AchievementFact> facts,
        IReadOnlyCollection<Guid> badgeGameIds,
        IReadOnlyCollection<GameContext> contexts)
    {
        var games = facts
            .Select(f => f.GameId)
            .Concat(badgeGameIds)
            .Distinct()
            .Order()
            .Select(gameId => ForGame(gameId, facts, badgeGameIds, contexts.FirstOrDefault(c => c.GameId == gameId)))
            .ToList();

        return new AchievementProgress(
            games,
            new CrossGameProgress(
                games.Count,
                MostGames(facts.Where(f => !f.IsEvolution)),
                MostGames(facts.Where(f => f.IsEvolution))));
    }

    private static GameProgress ForGame(
        Guid gameId,
        IReadOnlyCollection<AchievementFact> facts,
        IReadOnlyCollection<Guid> badgeGameIds,
        GameContext? context)
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
            badgeGameIds.Count(id => id == gameId),
            context?.DexRegistered ?? 0,
            context?.DexTotal ?? 0,
            context?.TotalBadges ?? 0);
    }

    private static int MostGames(IEnumerable<AchievementFact> facts) =>
        facts
            .GroupBy(f => f.SpeciesId)
            .Select(species => species.Select(f => f.GameId).Distinct().Count())
            .DefaultIfEmpty()
            .Max();
}
