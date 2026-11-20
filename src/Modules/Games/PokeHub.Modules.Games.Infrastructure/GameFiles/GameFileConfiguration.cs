using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Games.Domain.GameFiles;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Infrastructure.GameFiles;

internal sealed class GameFileConfiguration : IEntityTypeConfiguration<GameFile>
{
    public void Configure(EntityTypeBuilder<GameFile> builder)
    {
        builder.ToTable("game_files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.Kind).HasConversion<string>().HasMaxLength(20);

        builder.Property(f => f.FileName).HasMaxLength(255);

        builder.Property(f => f.ContentType).HasMaxLength(100);

        builder.Property(f => f.Sha256).HasMaxLength(64);

        // PostgreSQL bytea, stored out of line (TOAST) for large values like ROMs.
        builder.Property(f => f.Content);

        builder.Ignore(f => f.IsSingle);

        builder.HasOne<Game>().WithMany().HasForeignKey(f => f.GameId);

        builder.HasIndex(f => new { f.GameId, f.Kind });
    }
}
