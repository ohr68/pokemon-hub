using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Infrastructure.Species;

internal sealed class SpeciesConfiguration : IEntityTypeConfiguration<PokemonSpecies>
{
    public void Configure(EntityTypeBuilder<PokemonSpecies> builder)
    {
        builder.ToTable("species");

        builder.HasKey(s => s.Id);

        // The national Pokédex number comes from PokéAPI.
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Name).HasMaxLength(100);

        builder.Property(s => s.PrimaryType).HasMaxLength(20);

        builder.Property(s => s.SecondaryType).HasMaxLength(20);

        builder.HasIndex(s => s.Name);
    }
}
