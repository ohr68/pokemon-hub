using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Economy.Domain.Shop;

namespace PokeHub.Modules.Economy.Infrastructure.Shop;

internal sealed class ShopOfferConfiguration : IEntityTypeConfiguration<ShopOffer>
{
    public void Configure(EntityTypeBuilder<ShopOffer> builder)
    {
        builder.ToTable("shop_offers");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.SpeciesName).HasMaxLength(100);

        builder.Ignore(o => o.IsAvailable);

        // The last one can't be sold twice.
        builder.Property<uint>("Version").IsRowVersion();
    }
}

internal sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("purchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.SpeciesName).HasMaxLength(100);

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<ShopOffer>().WithMany().HasForeignKey(p => p.OfferId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.TrainerId, p.PurchasedOnUtc });
    }
}
