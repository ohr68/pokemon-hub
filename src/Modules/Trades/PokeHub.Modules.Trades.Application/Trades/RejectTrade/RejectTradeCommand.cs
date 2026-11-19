using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Trades.Application.Trades.RejectTrade;

public sealed record RejectTradeCommand(Guid TradeId, Guid TrainerId) : ICommand;
