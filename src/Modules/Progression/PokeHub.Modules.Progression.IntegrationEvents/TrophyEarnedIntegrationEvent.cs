using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Progression.IntegrationEvents;

/// <summary>A trainer earned a trophy (coins can build on it).</summary>
/// <param name="tier">bronze, silver or gold.</param>
/// <param name="isCrossGame">It rewards what the trainer did across their games.</param>
public sealed class TrophyEarnedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid trainerId,
    string trophyCode,
    string tier,
    bool isCrossGame = false) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TrainerId { get; init; } = trainerId;

    public string TrophyCode { get; init; } = trophyCode;

    public string Tier { get; init; } = tier;

    public bool IsCrossGame { get; init; } = isCrossGame;
}
