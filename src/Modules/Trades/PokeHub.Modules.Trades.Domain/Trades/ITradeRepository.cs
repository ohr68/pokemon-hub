namespace PokeHub.Modules.Trades.Domain.Trades;

public interface ITradeRepository
{
    Task<Trade?> GetAsync(Guid tradeId, CancellationToken cancellationToken = default);

    void Insert(Trade trade);
}
