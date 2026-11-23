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
    DexesCompleted = 4,
    BadgeSetsCompleted = 5,
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
    /// <summary>What connects a trainer's games. These pay the most (see the coin rules).</summary>
    public static readonly IReadOnlyList<CrossGameTrophy> All =
    [
        new("cross-games-2", "Novos horizontes", "Jogue 2 jogos conectados ao PokeHub.", TrophyTier.Bronze, CrossGameStat.GamesPlayed, 2),
        new("cross-games-3", "Explorador de regiões", "Jogue 3 jogos conectados ao PokeHub.", TrophyTier.Silver, CrossGameStat.GamesPlayed, 3),
        new("cross-games-5", "Viajante do mundo Pokémon", "Jogue 5 jogos conectados ao PokeHub.", TrophyTier.Gold, CrossGameStat.GamesPlayed, 5),

        new("cross-catch-2", "Velho conhecido", "Capture a mesma espécie em 2 jogos.", TrophyTier.Bronze, CrossGameStat.SameSpeciesCaught, 2),
        new("cross-catch-3", "Companheiro de jornada", "Capture a mesma espécie em 3 jogos.", TrophyTier.Silver, CrossGameStat.SameSpeciesCaught, 3),
        new("cross-catch-5", "Laço entre mundos", "Capture a mesma espécie em 5 jogos.", TrophyTier.Gold, CrossGameStat.SameSpeciesCaught, 5),

        new("cross-evolve-2", "Caminho conhecido", "Evolua um Pokémon para a mesma espécie em 2 jogos.", TrophyTier.Bronze, CrossGameStat.SameSpeciesEvolved, 2),
        new("cross-evolve-3", "Mestre da evolução", "Evolua um Pokémon para a mesma espécie em 3 jogos.", TrophyTier.Silver, CrossGameStat.SameSpeciesEvolved, 3),
        new("cross-evolve-5", "Evolução sem fronteiras", "Evolua um Pokémon para a mesma espécie em 5 jogos.", TrophyTier.Gold, CrossGameStat.SameSpeciesEvolved, 5),

        new("cross-dex-2", "Dupla Pokédex", "Complete a Pokédex de 2 jogos.", TrophyTier.Gold, CrossGameStat.DexesCompleted, 2),
        new("cross-badges-2", "Campeão de duas regiões", "Conquiste todas as insígnias de 2 jogos.", TrophyTier.Gold, CrossGameStat.BadgeSetsCompleted, 2),
    ];
}

public static class TrophyCatalog
{
    public static bool IsCrossGame(string storedCode) => CrossGameTrophies.All.Any(t => t.Code == storedCode);

    /// <summary>The tier of a stored trophy code, or null when no trophy has that code.</summary>
    public static TrophyTier? TierOf(string storedCode)
    {
        string code = storedCode.Split(':')[0];

        return Trophies.All.FirstOrDefault(t => t.Code == code)?.Tier
            ?? GameTrophies.All.FirstOrDefault(t => t.Code == code)?.Tier
            ?? CrossGameTrophies.All.FirstOrDefault(t => t.Code == code)?.Tier;
    }
}
