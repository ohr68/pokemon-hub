namespace PokeHub.Modules.Games.Domain.Stats;

public interface IPlayerGameStatsRepository
{
    Task<PlayerGameStats?> GetAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default);

    void Insert(PlayerGameStats stats);
}
