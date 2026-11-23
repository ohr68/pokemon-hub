namespace PokeHub.Modules.Games.PublicApi;

/// <summary>The counters games report about their players, for goals built on them.</summary>
public interface IPlayerGameCounters
{
    /// <summary>Each player's counters per game. With a trainer id, only theirs.</summary>
    Task<IReadOnlyCollection<PlayerGameCounters>> GetCountersAsync(Guid? trainerId = null, CancellationToken cancellationToken = default);

    /// <summary>The ids of the registered games among these slugs.</summary>
    Task<IReadOnlyDictionary<string, Guid>> GetGameIdsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken = default);
}

/// <param name="Counters">The game's own counters by name, such as hallOfFame.</param>
public sealed record PlayerGameCounters(Guid TrainerId, Guid GameId, IReadOnlyDictionary<string, long> Counters);
