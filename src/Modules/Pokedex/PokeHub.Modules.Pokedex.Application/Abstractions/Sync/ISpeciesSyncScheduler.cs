namespace PokeHub.Modules.Pokedex.Application.Abstractions.Sync;

/// <summary>
/// Starts the species sync job outside its schedule, for the manual sync.
/// </summary>
public interface ISpeciesSyncScheduler
{
    Task TriggerNowAsync(CancellationToken cancellationToken = default);
}
