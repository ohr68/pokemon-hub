using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Infrastructure.Trainers;

internal sealed class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable("trainers");

        builder.HasKey(t => t.Id);

        // Shares its identifier with the PokeHub user.
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(100);

        builder.Property(t => t.Code).HasMaxLength(TrainerCode.Length);

        builder.HasIndex(t => t.Code).IsUnique();

        builder.HasOne<PokemonSpecies>().WithMany().HasForeignKey(t => t.StarterSpeciesId);
    }
}
