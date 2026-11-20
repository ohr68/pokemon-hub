using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Progression.IntegrationEvents;

/// <summary>A trainer reached a new level (trophies and coins can build on it).</summary>
public sealed class TrainerLeveledUpIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid trainerId,
    int fromLevel,
    int toLevel) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TrainerId { get; init; } = trainerId;

    public int FromLevel { get; init; } = fromLevel;

    public int ToLevel { get; init; } = toLevel;
}
