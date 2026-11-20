using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>
/// A trainer's XP and level, across every game and the hub. The trainer id is the same as
/// in the Pokédex (and the user's id). Created with their first XP.
/// </summary>
public sealed class TrainerProgress : Entity
{
    private readonly List<XpEntry> _newEntries = [];

    private TrainerProgress()
    {
    }

    public Guid TrainerId { get; private set; }

    public long TotalXp { get; private set; }

    public int Level { get; private set; }

    public DateTime UpdatedOnUtc { get; private set; }

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

        int previousLevel = Level;
        TotalXp += amount;
        Level = XpRules.LevelFor(TotalXp);
        UpdatedOnUtc = utcNow;

        if (Level > previousLevel)
        {
            Raise(new TrainerLeveledUpDomainEvent(TrainerId, previousLevel, Level));
        }
    }
}
