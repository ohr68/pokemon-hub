using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>
/// A trainer's XP and level, across every game and the hub. The trainer id is the same as
/// in the Pokédex (and the user's id). Created with their first XP.
/// </summary>
public sealed class TrainerProgress : Entity
{
    private readonly List<XpEntry> _newEntries = [];

    private readonly List<TrainerTrophy> _trophies = [];

    private readonly List<TrainerGame> _games = [];

    private readonly List<TrainerBadge> _earnedBadges = [];

    private TrainerProgress()
    {
    }

    public Guid TrainerId { get; private set; }

    public long TotalXp { get; private set; }

    public int Level { get; private set; }

    // What trophies count, kept with each award.
    public int StartersChosen { get; private set; }

    public int Catches { get; private set; }

    public int ShinyCatches { get; private set; }

    public int SpeciesDiscovered { get; private set; }

    public int Trades { get; private set; }

    public int Evolutions { get; private set; }

    public int Badges { get; private set; }

    public DateTime UpdatedOnUtc { get; private set; }

    public IReadOnlyCollection<TrainerTrophy> Trophies => _trophies.AsReadOnly();

    // The games that reported badges, and the badges earned in them.
    public IReadOnlyCollection<TrainerGame> Games => _games.AsReadOnly();

    public IReadOnlyCollection<TrainerBadge> EarnedBadges => _earnedBadges.AsReadOnly();

    // Awards made since loading, for the repository to save.
    public IReadOnlyCollection<XpEntry> NewEntries => _newEntries.AsReadOnly();

    public static TrainerProgress Start(Guid trainerId, DateTime utcNow) => new()
    {
        TrainerId = trainerId,
        Level = 1,
        UpdatedOnUtc = utcNow,
    };

    /// <summary>
    /// Adds XP and levels up when it crosses a level's threshold. <paramref name="earnedOnUtc"/>
    /// is when it happened, which for replayed history is in the past.
    /// </summary>
    public void Earn(XpReason reason, string source, DateTime earnedOnUtc, DateTime utcNow)
    {
        int amount = XpRules.AmountFor(reason);

        if (amount <= 0)
        {
            return;
        }

        _newEntries.Add(XpEntry.Create(TrainerId, reason, amount, source, earnedOnUtc));

        Count(reason);

        int previousLevel = Level;
        TotalXp += amount;
        Level = XpRules.LevelFor(TotalXp);
        UpdatedOnUtc = utcNow;

        if (Level > previousLevel)
        {
            Raise(new TrainerLeveledUpDomainEvent(TrainerId, previousLevel, Level));
        }
    }

    /// <summary>
    /// Records what a game reported: every badge earned in it so far. New ones earn XP;
    /// badges are never removed (a save restored to before one keeps it).
    /// </summary>
    public void RecordBadges(Guid gameId, string gameName, int totalBadges, IEnumerable<(string Code, string Name)> badges, DateTime utcNow)
    {
        TrainerGame? game = _games.Find(g => g.GameId == gameId);

        if (game is null)
        {
            _games.Add(TrainerGame.Create(TrainerId, gameId, gameName, totalBadges));
        }
        else
        {
            game.Update(gameName, totalBadges);
        }

        foreach ((string code, string name) in badges)
        {
            if (_earnedBadges.Exists(b => b.GameId == gameId && b.Code == code))
            {
                continue;
            }

            _earnedBadges.Add(TrainerBadge.Create(TrainerId, gameId, code, name, utcNow));
            Earn(XpReason.EarnedBadge, $"badge:{gameId}:{code}", utcNow, utcNow);
            Raise(new BadgeEarnedDomainEvent(TrainerId, gameId, code));
        }
    }

    /// <summary>
    /// Every species the trainer ever had, however they got it. It comes from the Pokédex's
    /// history rather than from XP, because not every way of getting a Pokémon earns XP.
    /// </summary>
    public void SetSpeciesDiscovered(int count, DateTime utcNow)
    {
        if (count == SpeciesDiscovered)
        {
            return;
        }

        SpeciesDiscovered = count;
        UpdatedOnUtc = utcNow;
    }

    /// <summary>
    /// Awards every trophy whose target the trainer reached and doesn't have yet. Runs after
    /// each award, and on its own to catch up (a trophy added later, a replayed history).
    /// </summary>
    public void EarnTrophies(DateTime earnedOnUtc)
    {
        foreach (Trophy trophy in Domain.Trainers.Trophies.All)
        {
            if (Value(trophy.Stat) < trophy.Target || _trophies.Exists(t => t.TrophyCode == trophy.Code))
            {
                continue;
            }

            _trophies.Add(TrainerTrophy.Create(TrainerId, trophy.Code, earnedOnUtc));
            Raise(new TrophyEarnedDomainEvent(TrainerId, trophy.Code, trophy.Tier));
        }
    }

    /// <summary>
    /// Awards every per-game and cross-game trophy the trainer reached and doesn't have yet.
    /// Earned trophies are never removed, whatever later happens to the Pokémon.
    /// </summary>
    public void EarnAchievements(AchievementProgress achievements, DateTime earnedOnUtc) =>
        EarnAchievements(achievements, GameTrophies.All, CrossGameTrophies.All, earnedOnUtc);

    public void EarnAchievements(
        AchievementProgress achievements,
        IReadOnlyCollection<GameTrophy> gameTrophies,
        IReadOnlyCollection<CrossGameTrophy> crossGameTrophies,
        DateTime earnedOnUtc)
    {
        foreach (GameProgress game in achievements.Games)
        {
            foreach (GameTrophy trophy in gameTrophies.Where(t => game.Value(t.Stat) >= t.Target))
            {
                Award(trophy.CodeFor(game.GameId), trophy.Tier, earnedOnUtc);
            }
        }

        foreach (CrossGameTrophy trophy in crossGameTrophies.Where(t => achievements.CrossGame.Value(t.Stat) >= t.Target))
        {
            Award(trophy.Code, trophy.Tier, earnedOnUtc, isCrossGame: true);
        }
    }

    /// <summary>Awards every hand-made trophy the trainer reached and doesn't have yet.</summary>
    public void EarnSpecific(SpecificProgress specific, DateTime earnedOnUtc) =>
        EarnSpecific(specific, SpecificTrophies.All, earnedOnUtc);

    public void EarnSpecific(SpecificProgress specific, IReadOnlyCollection<SpecificTrophy> trophies, DateTime earnedOnUtc)
    {
        foreach (SpecificTrophy trophy in trophies.Where(t => specific.Value(t) >= t.Target))
        {
            Award(trophy.Code, trophy.Tier, earnedOnUtc);
        }
    }

    public int Value(TrainerStat stat) => stat switch
    {
        TrainerStat.StartersChosen => StartersChosen,
        TrainerStat.Catches => Catches,
        TrainerStat.ShinyCatches => ShinyCatches,
        TrainerStat.SpeciesDiscovered => SpeciesDiscovered,
        TrainerStat.Trades => Trades,
        TrainerStat.Evolutions => Evolutions,
        TrainerStat.Level => Level,
        TrainerStat.Badges => Badges,
        _ => 0,
    };

    private void Award(string code, TrophyTier tier, DateTime earnedOnUtc, bool isCrossGame = false)
    {
        if (_trophies.Exists(t => t.TrophyCode == code))
        {
            return;
        }

        _trophies.Add(TrainerTrophy.Create(TrainerId, code, earnedOnUtc));
        Raise(new TrophyEarnedDomainEvent(TrainerId, code, tier, isCrossGame));
    }

    private void Count(XpReason reason)
    {
        switch (reason)
        {
            case XpReason.ChoseStarter: StartersChosen++; break;
            case XpReason.CaughtPokemon: Catches++; break;
            case XpReason.CaughtShiny: ShinyCatches++; break;
            case XpReason.Traded: Trades++; break;
            case XpReason.Evolved: Evolutions++; break;
            case XpReason.EarnedBadge: Badges++; break;
        }
    }
}
