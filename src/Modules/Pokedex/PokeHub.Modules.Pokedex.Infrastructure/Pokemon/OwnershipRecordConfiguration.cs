using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Infrastructure.Pokemon;

internal sealed class OwnershipRecordConfiguration : IEntityTypeConfiguration<OwnershipRecord>
{
    public void Configure(EntityTypeBuilder<OwnershipRecord> builder)
    {
        builder.ToTable("ownership_history");

        builder.HasKey(r => r.Id);

        // Records are created with their id, so new records added to a loaded Pokémon's history
        // are always inserted rather than mistaken for existing rows.
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Change).HasConversion<string>().HasMaxLength(30);

        builder.HasOne<Trainer>().WithMany().HasForeignKey(r => r.TrainerId);

        builder.HasOne<PokemonSpecies>().WithMany().HasForeignKey(r => r.SpeciesId);

        builder.HasIndex(r => r.TradeId);

        // Pokédex progress reads distinct species per trainer and change.
        builder.HasIndex(r => new { r.TrainerId, r.Change, r.SpeciesId });
    }
}
