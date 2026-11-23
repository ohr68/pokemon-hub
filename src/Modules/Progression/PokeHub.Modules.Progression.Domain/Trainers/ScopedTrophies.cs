namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>What a per-game trophy counts.</summary>
public enum GameStat
{
    Catches = 1,
    Evolutions = 2,
    ShinyCatches = 3,
    SpeciesRegistered = 4,
    Badges = 5,

    // Share of the game's own Pokédex, and of its badges, from 0 to 100.
    DexPercent = 6,
    BadgePercent = 7,
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
    /// <summary>The set every game gets, with nothing to configure when a game is added.</summary>
    public static readonly IReadOnlyList<GameTrophy> All =
    [
        new("game-catch-1", "Primeira captura aqui", "Capture um Pokémon neste jogo.", TrophyTier.Bronze, GameStat.Catches, 1),
        new("game-catch-25", "Caçador da região", "Capture 25 Pokémon neste jogo.", TrophyTier.Bronze, GameStat.Catches, 25),
        new("game-catch-100", "Terror da grama alta", "Capture 100 Pokémon neste jogo.", TrophyTier.Silver, GameStat.Catches, 100),

        new("game-evolve-1", "Primeira evolução aqui", "Evolua um Pokémon neste jogo.", TrophyTier.Bronze, GameStat.Evolutions, 1),
        new("game-evolve-10", "Criador da região", "Evolua 10 Pokémon neste jogo.", TrophyTier.Silver, GameStat.Evolutions, 10),

        new("game-shiny-1", "Brilho na região", "Capture um shiny neste jogo.", TrophyTier.Silver, GameStat.ShinyCatches, 1),

        new("game-dex-25", "Pokédex iniciada", "Registre 25% da Pokédex deste jogo.", TrophyTier.Bronze, GameStat.DexPercent, 25),
        new("game-dex-50", "Meia Pokédex", "Registre 50% da Pokédex deste jogo.", TrophyTier.Silver, GameStat.DexPercent, 50),
        new("game-dex-100", "Pokédex completa", "Registre toda a Pokédex deste jogo.", TrophyTier.Gold, GameStat.DexPercent, 100),

        new("game-badge-1", "Primeira insígnia aqui", "Conquiste uma insígnia neste jogo.", TrophyTier.Bronze, GameStat.Badges, 1),
        new("game-badges-all", "Todas as insígnias", "Conquiste todas as insígnias deste jogo.", TrophyTier.Gold, GameStat.BadgePercent, 100),
    ];
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
