using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Games.IntegrationEvents;

/// <summary>A game reported new counters for a player, which goals may build on.</summary>
public sealed class GameStatsReportedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid trainerId,
    Guid gameId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TrainerId { get; init; } = trainerId;

    public Guid GameId { get; init; } = gameId;
}
