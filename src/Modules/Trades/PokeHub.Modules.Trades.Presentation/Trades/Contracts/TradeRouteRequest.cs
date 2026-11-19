namespace PokeHub.Modules.Trades.Presentation.Trades.Contracts;

// Bound from the route: trades/{tradeId}/...
internal sealed class TradeRouteRequest
{
    public Guid TradeId { get; init; }
}
