using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Trades.Application.Trades.CancelTrade;

public sealed record CancelTradeCommand(Guid TradeId, Guid TrainerId) : ICommand;
