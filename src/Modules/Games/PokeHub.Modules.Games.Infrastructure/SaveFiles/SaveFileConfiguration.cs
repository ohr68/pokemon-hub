using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveFiles;

namespace PokeHub.Modules.Games.Infrastructure.SaveFiles;

internal sealed class SaveFileConfiguration : IEntityTypeConfiguration<SaveFile>
{
    public void Configure(EntityTypeBuilder<SaveFile> builder)
    {
        builder.ToTable("save_files");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Sha256).HasMaxLength(64);

        // PostgreSQL bytea; values this size are stored compressed, out of line (TOAST).
        builder.Property(s => s.Content);

        // A game with players' saves can't be deleted by accident.
        builder.HasOne<Game>().WithMany().HasForeignKey(s => s.GameId).OnDelete(DeleteBehavior.Restrict);

        // The trainer lives in the Pokédex module, so there is no foreign key to it.
        builder.HasIndex(s => new { s.TrainerId, s.GameId, s.UploadedOnUtc });
    }
}
