namespace PokeHub.Modules.Games.PublicApi;

/// <summary>
/// Which Pokémon exist in each game, so none is sent to a game that doesn't have it.
/// </summary>
public interface IGameSpeciesLimits
{
    /// <summary>
    /// The last National Dex number of each game. A game without a limit, or unknown, is left out.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetMaxSpeciesIdsAsync(
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken = default);
}
