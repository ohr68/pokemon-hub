using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Infrastructure.Inbox;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;

namespace PokeHub.Modules.Pokedex.Infrastructure.Database;

public sealed class PokedexDbContext(DbContextOptions<PokedexDbContext> options) : DbContext(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Pokedex);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
    }
}
