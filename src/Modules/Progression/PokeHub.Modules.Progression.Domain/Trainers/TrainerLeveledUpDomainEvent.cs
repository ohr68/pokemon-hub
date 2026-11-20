using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Progression.Domain.Trainers;

public sealed class TrainerLeveledUpDomainEvent(Guid trainerId, int fromLevel, int toLevel) : DomainEvent
{
    public Guid TrainerId { get; init; } = trainerId;

    public int FromLevel { get; init; } = fromLevel;

    public int ToLevel { get; init; } = toLevel;
}
