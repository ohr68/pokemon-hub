namespace PokeHub.Modules.Games.Domain.GameFiles;

public interface IGameFileRepository
{
    Task<GameFile?> GetAsync(Guid fileId, CancellationToken cancellationToken = default);

    // The game's cover, icon or ROM, of which there is at most one.
    Task<GameFile?> GetSingleAsync(Guid gameId, GameFileKind kind, CancellationToken cancellationToken = default);

    void Insert(GameFile file);

    void Remove(GameFile file);
}
