using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Trades.Domain.Trades;

namespace PokeHub.Modules.Trades.Infrastructure.Trades;

internal sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("trades");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.ProposerName).HasMaxLength(100);

        builder.Property(t => t.RecipientName).HasMaxLength(100);

        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(t => t.FailureReason).HasMaxLength(500);

        // Stored as offered_* and requested_* columns on the trade itself.
        builder.OwnsOne(t => t.Offered, ConfigureSnapshot);

        builder.OwnsOne(t => t.Requested, ConfigureSnapshot);

        builder.HasIndex(t => new { t.ProposerId, t.CreatedOnUtc });

        builder.HasIndex(t => new { t.RecipientId, t.CreatedOnUtc });
    }

    private static void ConfigureSnapshot(OwnedNavigationBuilder<Trade, TradedPokemon> pokemon)
    {
        pokemon.Property(p => p.SpeciesName).HasMaxLength(100);

        pokemon.Property(p => p.Nickname).HasMaxLength(12);
    }
}
