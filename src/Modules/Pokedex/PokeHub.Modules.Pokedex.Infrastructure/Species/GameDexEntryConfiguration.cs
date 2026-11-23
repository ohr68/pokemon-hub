using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Infrastructure.Species;

internal sealed class GameDexEntryConfiguration : IEntityTypeConfiguration<GameDexEntry>
{
    public void Configure(EntityTypeBuilder<GameDexEntry> builder)
    {
        builder.ToTable("game_dex_entries");

        builder.HasKey(e => new { e.GameSlug, e.SpeciesId });

        builder.Property(e => e.GameSlug).HasMaxLength(50);

        builder.HasOne<PokemonSpecies>().WithMany().HasForeignKey(e => e.SpeciesId).OnDelete(DeleteBehavior.Cascade);
    }
}
