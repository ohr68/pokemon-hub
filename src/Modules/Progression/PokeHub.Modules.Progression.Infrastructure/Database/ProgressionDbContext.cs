using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Infrastructure.Inbox;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Progression.Application.Abstractions.Data;
using PokeHub.Modules.Progression.Domain.Trainers;
using PokeHub.Modules.Progression.Infrastructure.Trainers;

namespace PokeHub.Modules.Progression.Infrastructure.Database;

public sealed class ProgressionDbContext(DbContextOptions<ProgressionDbContext> options) : DbContext(options), IUnitOfWork
{
    internal DbSet<TrainerProgress> TrainerProgress { get; set; }

    internal DbSet<XpEntry> XpEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Progression);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new TrainerProgressConfiguration());
        modelBuilder.ApplyConfiguration(new XpEntryConfiguration());
    }
}
