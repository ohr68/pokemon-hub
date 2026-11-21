using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Infrastructure.Inbox;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Economy.Application.Abstractions.Data;
using PokeHub.Modules.Economy.Domain.Shop;
using PokeHub.Modules.Economy.Domain.Wallets;
using PokeHub.Modules.Economy.Infrastructure.Shop;
using PokeHub.Modules.Economy.Infrastructure.Wallets;

namespace PokeHub.Modules.Economy.Infrastructure.Database;

public sealed class EconomyDbContext(DbContextOptions<EconomyDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Wallet> Wallets { get; set; }

    internal DbSet<CoinEntry> CoinEntries { get; set; }

    internal DbSet<ShopOffer> ShopOffers { get; set; }

    internal DbSet<Purchase> Purchases { get; set; }

    internal DbSet<ItemOffer> ItemOffers { get; set; }

    internal DbSet<ItemPurchase> ItemPurchases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Economy);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new CoinEntryConfiguration());
        modelBuilder.ApplyConfiguration(new ShopOfferConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseConfiguration());
        modelBuilder.ApplyConfiguration(new ItemOfferConfiguration());
        modelBuilder.ApplyConfiguration(new ItemPurchaseConfiguration());
    }
}
