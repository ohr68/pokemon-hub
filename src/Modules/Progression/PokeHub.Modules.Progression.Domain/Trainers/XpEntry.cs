namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>
/// One XP award. <see cref="Source"/> names what earned it (pokemon-obtained:{id},
/// trade:{id}, ...), so the same thing never pays twice.
/// </summary>
public sealed class XpEntry
{
    private XpEntry()
    {
    }

    public Guid Id { get; private set; }

    public Guid TrainerId { get; private set; }

    public XpReason Reason { get; private set; }

    public int Amount { get; private set; }

    public string Source { get; private set; }

    public DateTime EarnedOnUtc { get; private set; }

    internal static XpEntry Create(Guid trainerId, XpReason reason, int amount, string source, DateTime utcNow) => new()
    {
        Id = Guid.CreateVersion7(),
        TrainerId = trainerId,
        Reason = reason,
        Amount = amount,
        Source = source,
        EarnedOnUtc = utcNow,
    };
}
