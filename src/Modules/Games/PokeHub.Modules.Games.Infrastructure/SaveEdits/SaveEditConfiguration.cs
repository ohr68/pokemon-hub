using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveEdits;

namespace PokeHub.Modules.Games.Infrastructure.SaveEdits;

internal sealed class SaveEditConfiguration : IEntityTypeConfiguration<SaveEdit>
{
    public void Configure(EntityTypeBuilder<SaveEdit> builder)
    {
        builder.ToTable("save_edits");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        // PostgreSQL arrays: uuid[] and text[].
        builder.Property(s => s.DeliveredPokemonIds);

        builder.Property(s => s.DeliveredCatchKeys);

        builder.Property(s => s.WithdrawnCatchKeys);

        builder.HasOne<Game>().WithMany().HasForeignKey(s => s.GameId).OnDelete(DeleteBehavior.Restrict);

        // The trainer lives in the Pokédex module, so there is no foreign key to it.
        builder.HasIndex(s => new { s.TrainerId, s.GameId, s.SettledOnUtc });
    }
}
