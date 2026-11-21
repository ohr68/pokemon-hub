using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Economy.Domain.Wallets;

namespace PokeHub.Modules.Economy.Infrastructure.Wallets;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        // The Pokédex's trainer id; the trainer lives in that module, so no foreign key.
        builder.HasKey(w => w.TrainerId);

        builder.Property(w => w.TrainerId).ValueGeneratedNever();

        builder.Ignore(w => w.NewEntries);

        // Two purchases at the same moment can't both spend the same coins.
        builder.Property<uint>("Version").IsRowVersion();
    }
}

internal sealed class CoinEntryConfiguration : IEntityTypeConfiguration<CoinEntry>
{
    public void Configure(EntityTypeBuilder<CoinEntry> builder)
    {
        builder.ToTable("coin_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Reason).HasConversion<string>().HasMaxLength(30);

        builder.Property(e => e.Source).HasMaxLength(150);

        builder.HasOne<Wallet>().WithMany().HasForeignKey(e => e.TrainerId);

        // Each thing pays (or charges) once.
        builder.HasIndex(e => new { e.TrainerId, e.Source }).IsUnique();

        builder.HasIndex(e => new { e.TrainerId, e.OccurredOnUtc });
    }
}
