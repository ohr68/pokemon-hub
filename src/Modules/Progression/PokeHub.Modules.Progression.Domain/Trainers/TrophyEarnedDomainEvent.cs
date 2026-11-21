using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Progression.Domain.Trainers;

public sealed class TrophyEarnedDomainEvent(Guid trainerId, string trophyCode, TrophyTier tier) : DomainEvent
{
    public Guid TrainerId { get; init; } = trainerId;

    public string TrophyCode { get; init; } = trophyCode;

    public TrophyTier Tier { get; init; } = tier;
}
