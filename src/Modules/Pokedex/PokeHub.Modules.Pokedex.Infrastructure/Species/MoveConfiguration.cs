using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Infrastructure.Species;

internal sealed class MoveConfiguration : IEntityTypeConfiguration<Move>
{
    public void Configure(EntityTypeBuilder<Move> builder)
    {
        builder.ToTable("moves");

        builder.HasKey(m => m.Id);

        // PokéAPI's move id.
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Name).HasMaxLength(50);
    }
}
