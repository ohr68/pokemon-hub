using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Saves;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Infrastructure.Saves;

internal sealed class SaveConfiguration : IEntityTypeConfiguration<Save>
{
    public void Configure(EntityTypeBuilder<Save> builder)
    {
        builder.ToTable("saves");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.GameName).HasMaxLength(100);

        builder.HasOne<Trainer>().WithMany().HasForeignKey(s => s.TrainerId);

        // One save per trainer per game.
        builder.HasIndex(s => new { s.TrainerId, s.GameId }).IsUnique();
    }
}
