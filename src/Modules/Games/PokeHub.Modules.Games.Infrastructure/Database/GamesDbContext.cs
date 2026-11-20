using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Infrastructure.Inbox;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveFiles;
using PokeHub.Modules.Games.Infrastructure.Games;
using PokeHub.Modules.Games.Infrastructure.SaveFiles;

namespace PokeHub.Modules.Games.Infrastructure.Database;

public sealed class GamesDbContext(DbContextOptions<GamesDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<Game> Games { get; set; }

    internal DbSet<SaveFile> SaveFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Games);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new GameConfiguration());
        modelBuilder.ApplyConfiguration(new SaveFileConfiguration());
    }
}
