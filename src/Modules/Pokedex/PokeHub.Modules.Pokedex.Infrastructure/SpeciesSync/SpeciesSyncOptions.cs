namespace PokeHub.Modules.Pokedex.Infrastructure.SpeciesSync;

internal sealed class SpeciesSyncOptions
{
    /// <summary>
    /// Quartz cron expression for the scheduled sync. Empty disables the schedule, leaving only
    /// the manual sync. PokéAPI rarely changes, so the default is weekly (Sunday 03:00 UTC).
    /// </summary>
    public string? Cron { get; init; } = "0 0 3 ? * SUN";

    /// <summary>
    /// Fills an empty catalog when the API starts, so a new database is usable right away.
    /// </summary>
    public bool SyncOnStartupWhenEmpty { get; init; } = true;
}
