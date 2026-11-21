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

        builder.Property(s => s.GrowthRate).HasMaxLength(30);

        builder.HasIndex(s => s.Gen3Index);

        builder.Ignore(s => s.IsRare);

        // One row per move, replaced as a whole when a sync changes the learnset.
        builder.OwnsMany(s => s.Gen3LevelUpMoves, moves =>
        {
            moves.ToTable("species_gen3_level_up_moves");
            moves.WithOwner().HasForeignKey("SpeciesId");
            moves.HasKey("SpeciesId", nameof(LevelUpMove.Order));
            moves.Property(m => m.Order).ValueGeneratedNever();
            moves.HasOne<Move>().WithMany().HasForeignKey(m => m.MoveId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation(s => s.Gen3LevelUpMoves).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
