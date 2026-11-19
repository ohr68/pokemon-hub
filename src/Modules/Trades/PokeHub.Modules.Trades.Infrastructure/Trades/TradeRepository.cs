using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Trades.Domain.Trades;
using PokeHub.Modules.Trades.Infrastructure.Database;

namespace PokeHub.Modules.Trades.Infrastructure.Trades;

internal sealed class TradeRepository(TradesDbContext context) : ITradeRepository
{
    public async Task<Trade?> GetAsync(Guid tradeId, CancellationToken cancellationToken = default)
    {
        return await context.Trades.SingleOrDefaultAsync(t => t.Id == tradeId, cancellationToken);
    }

    public void Insert(Trade trade)
    {
        context.Trades.Add(trade);
    }
}
