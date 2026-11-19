using PokeHub.Modules.Pokedex.Application.Abstractions.Sync;
using Quartz;

namespace PokeHub.Modules.Pokedex.Infrastructure.SpeciesSync;

internal sealed class SpeciesSyncScheduler(ISchedulerFactory schedulerFactory) : ISpeciesSyncScheduler
{
    public async Task TriggerNowAsync(CancellationToken cancellationToken = default)
    {
        IScheduler scheduler = await schedulerFactory.GetScheduler(cancellationToken);

        await scheduler.TriggerJob(SyncSpeciesJob.Key, cancellationToken);
    }
}
