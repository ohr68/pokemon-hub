using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Progression.IntegrationEvents;

/// <summary>A trainer earned a trophy (coins can build on it).</summary>
/// <param name="tier">bronze, silver or gold.</param>
public sealed class TrophyEarnedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid trainerId,
    string trophyCode,
    string tier) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TrainerId { get; init; } = trainerId;

    public string TrophyCode { get; init; } = trophyCode;

    public string Tier { get; init; } = tier;
}
