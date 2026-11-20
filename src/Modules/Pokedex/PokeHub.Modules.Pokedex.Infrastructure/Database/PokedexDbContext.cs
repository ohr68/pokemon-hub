using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Infrastructure.Inbox;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;
using PokeHub.Modules.Pokedex.Infrastructure.Saves;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;
using PokeHub.Modules.Pokedex.Infrastructure.Pokemon;
using PokeHub.Modules.Pokedex.Infrastructure.Species;
using PokeHub.Modules.Pokedex.Infrastructure.Trainers;

namespace PokeHub.Modules.Pokedex.Infrastructure.Database;

public sealed class PokedexDbContext(DbContextOptions<PokedexDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<PokemonSpecies> Species { get; set; }

    internal DbSet<Trainer> Trainers { get; set; }

    internal DbSet<OwnedPokemon> Pokemon { get; set; }

    internal DbSet<Save> Saves { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Pokedex);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new SpeciesConfiguration());
        modelBuilder.ApplyConfiguration(new TrainerConfiguration());
        modelBuilder.ApplyConfiguration(new OwnedPokemonConfiguration());
        modelBuilder.ApplyConfiguration(new OwnershipRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SaveConfiguration());
        modelBuilder.ApplyConfiguration(new GrowthRateConfiguration());
        modelBuilder.ApplyConfiguration(new MoveConfiguration());
    }
}
