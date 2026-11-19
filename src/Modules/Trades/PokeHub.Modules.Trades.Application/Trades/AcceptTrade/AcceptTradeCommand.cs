using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Trades.Application.Trades.AcceptTrade;

public sealed record AcceptTradeCommand(Guid TradeId, Guid TrainerId) : ICommand;
