namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>
/// How much XP each action is worth and how much each level takes: the one place to tune
/// trainer progression.
/// </summary>
public static class XpRules
{
    public const int MaxLevel = 100;

    public static int AmountFor(XpReason reason) => reason switch
    {
        XpReason.ChoseStarter => 50,
        XpReason.CaughtPokemon => 10,
        // On top of the catch itself.
        XpReason.CaughtShiny => 50,
        // The first Pokémon of a species the trainer never had, on top of the catch.
        XpReason.NewSpecies => 25,
        XpReason.Traded => 20,
        XpReason.Evolved => 15,
        // A gym badge in any game.
        XpReason.EarnedBadge => 100,
        _ => 0,
    };

    /// <summary>
    /// The XP from one level to the next: 100 from level 1 to 2, then 50 more for each level
    /// (150 to reach 3, 200 to reach 4, ...).
    /// </summary>
    public static int XpToNextLevel(int level) => 100 + 50 * (level - 1);

    /// <summary>The total XP needed to reach a level.</summary>
    public static long TotalXpForLevel(int level)
    {
        long steps = level - 1;
        return 100 * steps + 25 * steps * (steps - 1);
    }

    public static int LevelFor(long totalXp)
    {
        int level = 1;
        while (level < MaxLevel && totalXp >= TotalXpForLevel(level + 1))
        {
            level++;
        }

        return level;
    }
}
