using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Games.Domain.GameFiles;
using PokeHub.Modules.Games.Infrastructure.Database;

namespace PokeHub.Modules.Games.Infrastructure.GameFiles;

internal sealed class GameFileRepository(GamesDbContext context) : IGameFileRepository
{
    public Task<GameFile?> GetAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return context.GameFiles.SingleOrDefaultAsync(f => f.Id == fileId, cancellationToken);
    }

    public Task<GameFile?> GetSingleAsync(Guid gameId, GameFileKind kind, CancellationToken cancellationToken = default)
    {
        return context.GameFiles.FirstOrDefaultAsync(f => f.GameId == gameId && f.Kind == kind, cancellationToken);
    }

    public void Insert(GameFile file)
    {
        context.GameFiles.Add(file);
    }

    public void Remove(GameFile file)
    {
        context.GameFiles.Remove(file);
    }
}
