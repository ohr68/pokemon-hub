using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Progression.IntegrationEvents;

/// <summary>A trainer earned a new gym badge in a game (coins build on it).</summary>
public sealed class BadgeEarnedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid trainerId,
    Guid gameId,
    string badgeCode) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TrainerId { get; init; } = trainerId;

    public Guid GameId { get; init; } = gameId;

    public string BadgeCode { get; init; } = badgeCode;
}
