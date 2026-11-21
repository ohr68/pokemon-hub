using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Economy.Domain.Shop;

namespace PokeHub.Modules.Economy.Infrastructure.Shop;

internal sealed class ItemOfferConfiguration : IEntityTypeConfiguration<ItemOffer>
{
    public void Configure(EntityTypeBuilder<ItemOffer> builder)
    {
        builder.ToTable("item_offers");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.ItemName).HasMaxLength(50);

        builder.Property(o => o.Pocket).HasMaxLength(20);

        builder.Ignore(o => o.IsAvailable);

        // The last one can't be sold twice.
        builder.Property<uint>("Version").IsRowVersion();
    }
}

internal sealed class ItemPurchaseConfiguration : IEntityTypeConfiguration<ItemPurchase>
{
    public void Configure(EntityTypeBuilder<ItemPurchase> builder)
    {
        builder.ToTable("item_purchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.ItemName).HasMaxLength(50);

        builder.Property(p => p.Pocket).HasMaxLength(20);

        builder.HasOne<ItemOffer>().WithMany().HasForeignKey(p => p.OfferId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.TrainerId, p.DeliveredOnUtc });
    }
}
