using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Progression.Domain.Trainers;

public sealed class BadgeEarnedDomainEvent(Guid trainerId, Guid gameId, string badgeCode) : DomainEvent
{
    public Guid TrainerId { get; init; } = trainerId;

    public Guid GameId { get; init; } = gameId;

    public string BadgeCode { get; init; } = badgeCode;
}
