using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Infrastructure.Games;

internal sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.Name).HasMaxLength(100);

        builder.Property(g => g.Slug).HasMaxLength(40);

        builder.Property(g => g.ClientId).HasMaxLength(50);

        builder.Property(g => g.Url).HasMaxLength(500);

        // Stored as a PostgreSQL text[] column.
        builder.Property(g => g.WebOrigins);

        builder.HasIndex(g => g.Slug).IsUnique();

        builder.HasIndex(g => g.ClientId).IsUnique();
    }
}
