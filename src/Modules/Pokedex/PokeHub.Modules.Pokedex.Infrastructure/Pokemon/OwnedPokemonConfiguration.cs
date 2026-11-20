using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;
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

        // Where the Pokémon is: a save, or the hub when empty.
        builder.HasOne<Save>().WithMany().HasForeignKey(p => p.SaveId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.CatchKey).HasMaxLength(100);

        // A game can't record the same catch twice.
        builder.HasIndex(p => new { p.CaughtInGameId, p.CatchKey })
            .IsUnique()
            .HasFilter("catch_key IS NOT NULL");

        // PostgreSQL's xmin system column changes on every update. As a concurrency token it
        // makes a save fail when the Pokémon changed since it was read, so two trades accepted
        // at the same moment can't both move it.
        builder.Property<uint>("Version").IsRowVersion();
    }
}
