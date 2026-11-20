using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Progression.Domain.Trainers;
using PokeHub.Modules.Progression.Infrastructure.Database;

namespace PokeHub.Modules.Progression.Infrastructure.Trainers;

internal sealed class TrainerProgressRepository(ProgressionDbContext context) : ITrainerProgressRepository
{
    public Task<TrainerProgress?> GetAsync(Guid trainerId, CancellationToken cancellationToken = default)
    {
        return context.TrainerProgress.SingleOrDefaultAsync(p => p.TrainerId == trainerId, cancellationToken);
    }

    public async Task<IReadOnlySet<string>> GetEarnedSourcesAsync(
        Guid trainerId,
        IReadOnlyCollection<string> sources,
        CancellationToken cancellationToken = default)
    {
        List<string> earned = await context.XpEntries
            .Where(e => e.TrainerId == trainerId && sources.Contains(e.Source))
            .Select(e => e.Source)
            .ToListAsync(cancellationToken);

        return earned.ToHashSet(StringComparer.Ordinal);
    }

    public void Insert(TrainerProgress progress)
    {
        context.TrainerProgress.Add(progress);
    }

    public void InsertEntries(IEnumerable<XpEntry> entries)
    {
        context.XpEntries.AddRange(entries);
    }
}
