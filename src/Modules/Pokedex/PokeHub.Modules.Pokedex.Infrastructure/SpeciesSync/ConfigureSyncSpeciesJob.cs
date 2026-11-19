using Microsoft.Extensions.Options;
using Quartz;

namespace PokeHub.Modules.Pokedex.Infrastructure.SpeciesSync;

internal sealed class ConfigureSyncSpeciesJob(IOptions<SpeciesSyncOptions> syncOptions)
    : IConfigureOptions<QuartzOptions>
{
    private readonly SpeciesSyncOptions _syncOptions = syncOptions.Value;

    public void Configure(QuartzOptions options)
    {
        // Durable so the manual sync can start it even when no schedule is configured.
        options.AddJob<SyncSpeciesJob>(job => job.WithIdentity(SyncSpeciesJob.Key).StoreDurably());

        if (!string.IsNullOrWhiteSpace(_syncOptions.Cron))
        {
            options.AddTrigger(trigger => trigger
                .ForJob(SyncSpeciesJob.Key)
                .WithIdentity($"{SyncSpeciesJob.Key.Name}.Schedule")
                .WithCronSchedule(_syncOptions.Cron, cron => cron.InTimeZone(TimeZoneInfo.Utc)));
        }

        if (_syncOptions.SyncOnStartupWhenEmpty)
        {
            options.AddTrigger(trigger => trigger
                .ForJob(SyncSpeciesJob.Key)
                .WithIdentity($"{SyncSpeciesJob.Key.Name}.Startup")
                .UsingJobData(SyncSpeciesJob.OnlyIfEmptyKey, true)
                .StartNow());
        }
    }
}
