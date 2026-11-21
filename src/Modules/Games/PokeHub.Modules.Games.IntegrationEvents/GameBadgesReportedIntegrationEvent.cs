using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Games.IntegrationEvents;

/// <summary>
/// A game reported the badges a player has earned in it (all of them so far, not only new
/// ones). Progression records them on the trainer.
/// </summary>
/// <param name="totalBadges">How many badges the game has in all.</param>
public sealed class GameBadgesReportedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid trainerId,
    Guid gameId,
    string gameName,
    int totalBadges,
    IReadOnlyCollection<ReportedBadge> badges) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TrainerId { get; init; } = trainerId;

    public Guid GameId { get; init; } = gameId;

    public string GameName { get; init; } = gameName;

    public int TotalBadges { get; init; } = totalBadges;

    public IReadOnlyCollection<ReportedBadge> Badges { get; init; } = badges;
}

/// <param name="Code">The game's own id for the badge, stable across reports.</param>
public sealed record ReportedBadge(string Code, string Name);
