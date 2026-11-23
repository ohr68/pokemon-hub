namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>What a per-game trophy counts.</summary>
public enum GameStat
{
    Catches = 1,
    Evolutions = 2,
    ShinyCatches = 3,
    SpeciesRegistered = 4,
    Badges = 5,
}

/// <summary>What a cross-game trophy counts.</summary>
public enum CrossGameStat
{
    GamesPlayed = 1,
    SameSpeciesCaught = 2,
    SameSpeciesEvolved = 3,
}

/// <summary>A trophy every game offers, earned separately in each.</summary>
/// <param name="Code">Stable id: never rename one, and never use a colon in it.</param>
public sealed record GameTrophy(string Code, string Name, string Description, TrophyTier Tier, GameStat Stat, int Target)
{
    /// <summary>How it's stored for a trainer: one per game.</summary>
    public string CodeFor(Guid gameId) => $"{Code}:{gameId}";
}

/// <summary>A trophy earned once, for what the trainer did across their games.</summary>
/// <param name="Code">Stable id: never rename one, and never use a colon in it.</param>
public sealed record CrossGameTrophy(string Code, string Name, string Description, TrophyTier Tier, CrossGameStat Stat, int Target);

public static class GameTrophies
{
    public static readonly IReadOnlyList<GameTrophy> All = [];
}

public static class CrossGameTrophies
{
    public static readonly IReadOnlyList<CrossGameTrophy> All = [];
}

public static class TrophyCatalog
{
    /// <summary>The tier of a stored trophy code, or null when no trophy has that code.</summary>
    public static TrophyTier? TierOf(string storedCode)
    {
        string code = storedCode.Split(':')[0];

        return Trophies.All.FirstOrDefault(t => t.Code == code)?.Tier
            ?? GameTrophies.All.FirstOrDefault(t => t.Code == code)?.Tier
            ?? CrossGameTrophies.All.FirstOrDefault(t => t.Code == code)?.Tier;
    }
}
