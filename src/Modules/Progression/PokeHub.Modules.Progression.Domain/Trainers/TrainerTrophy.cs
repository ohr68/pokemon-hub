namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>A trophy a trainer earned (see <see cref="Trophies"/>).</summary>
public sealed class TrainerTrophy
{
    private TrainerTrophy()
    {
    }

    public Guid TrainerId { get; private set; }

    public string TrophyCode { get; private set; }

    public DateTime EarnedOnUtc { get; private set; }

    internal static TrainerTrophy Create(Guid trainerId, string trophyCode, DateTime earnedOnUtc) => new()
    {
        TrainerId = trainerId,
        TrophyCode = trophyCode,
        EarnedOnUtc = earnedOnUtc,
    };
}
