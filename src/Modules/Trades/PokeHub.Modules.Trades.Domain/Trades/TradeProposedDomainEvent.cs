using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Trades.Domain.Trades;

public sealed class TradeProposedDomainEvent(Guid tradeId, Guid proposerId, Guid recipientId) : DomainEvent
{
    public Guid TradeId { get; init; } = tradeId;

    public Guid ProposerId { get; init; } = proposerId;

    public Guid RecipientId { get; init; } = recipientId;
}
