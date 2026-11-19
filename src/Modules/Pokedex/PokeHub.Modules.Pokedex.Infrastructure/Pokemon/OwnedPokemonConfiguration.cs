using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Infrastructure.Pokemon;

internal sealed class OwnedPokemonConfiguration : IEntityTypeConfiguration<OwnedPokemon>
{
    public void Configure(EntityTypeBuilder<OwnedPokemon> builder)
    {
        builder.ToTable("pokemon");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nickname).HasMaxLength(12);

        builder.Property(p => p.Origin).HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<PokemonSpecies>().WithMany().HasForeignKey(p => p.SpeciesId);

        builder.HasOne<Trainer>().WithMany().HasForeignKey(p => p.TrainerId);

        builder.HasOne<Trainer>().WithMany().HasForeignKey(p => p.OriginalTrainerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.History).WithOne().HasForeignKey(r => r.PokemonId);

        builder.Navigation(p => p.History).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new { p.TrainerId, p.Status });
    }
}
