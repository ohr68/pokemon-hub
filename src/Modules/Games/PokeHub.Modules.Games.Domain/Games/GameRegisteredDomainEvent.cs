using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.Games;

public sealed class GameRegisteredDomainEvent(Guid gameId, string clientId) : DomainEvent
{
    public Guid GameId { get; init; } = gameId;

    public string ClientId { get; init; } = clientId;
}
