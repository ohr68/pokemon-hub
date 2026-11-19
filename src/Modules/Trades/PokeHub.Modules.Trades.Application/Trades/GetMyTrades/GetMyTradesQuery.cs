using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Trades.Application.Trades.GetMyTrades;

/// <summary>
/// Every trade the trainer proposed or received, newest first.
/// </summary>
public sealed record GetMyTradesQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<TradeResponse>>;
