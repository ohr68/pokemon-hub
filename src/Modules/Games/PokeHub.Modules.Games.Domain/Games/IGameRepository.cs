namespace PokeHub.Modules.Games.Domain.Games;

public interface IGameRepository
{
    Task<Game?> GetAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<Game?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    void Insert(Game game);
}
