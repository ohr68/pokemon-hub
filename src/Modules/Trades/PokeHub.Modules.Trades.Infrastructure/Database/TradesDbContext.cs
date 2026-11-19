using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Infrastructure.Inbox;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Trades.Application.Abstractions.Data;
using PokeHub.Modules.Trades.Domain.Trades;
using PokeHub.Modules.Trades.Infrastructure.Trades;

namespace PokeHub.Modules.Trades.Infrastructure.Database;

public sealed class TradesDbContext(DbContextOptions<TradesDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Trades);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new TradeConfiguration());
    }
}
