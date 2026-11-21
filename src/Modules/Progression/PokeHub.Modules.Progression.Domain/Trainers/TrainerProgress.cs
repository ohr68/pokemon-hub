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

    public DateTime UpdatedOnUtc { get; private set; }

    public IReadOnlyCollection<TrainerTrophy> Trophies => _trophies.AsReadOnly();

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

    public int Value(TrainerStat stat) => stat switch
    {
        TrainerStat.StartersChosen => StartersChosen,
        TrainerStat.Catches => Catches,
        TrainerStat.ShinyCatches => ShinyCatches,
        TrainerStat.SpeciesDiscovered => SpeciesDiscovered,
        TrainerStat.Trades => Trades,
        TrainerStat.Evolutions => Evolutions,
        TrainerStat.Level => Level,
        _ => 0,
    };

    private void Count(XpReason reason)
    {
        switch (reason)
        {
            case XpReason.ChoseStarter: StartersChosen++; break;
            case XpReason.CaughtPokemon: Catches++; break;
            case XpReason.CaughtShiny: ShinyCatches++; break;
            case XpReason.NewSpecies: SpeciesDiscovered++; break;
            case XpReason.Traded: Trades++; break;
            case XpReason.Evolved: Evolutions++; break;
        }
    }
}
