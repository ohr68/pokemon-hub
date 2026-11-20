using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Games.Domain.SaveFiles;
using PokeHub.Modules.Games.Infrastructure.Database;

namespace PokeHub.Modules.Games.Infrastructure.SaveFiles;

internal sealed class SaveFileRepository(GamesDbContext context) : ISaveFileRepository
{
    public Task<SaveFile?> GetLatestAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return context.SaveFiles
            .Where(s => s.TrainerId == trainerId && s.GameId == gameId)
            .OrderByDescending(s => s.UploadedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Insert(SaveFile saveFile)
    {
        context.SaveFiles.Add(saveFile);
    }

    public async Task RemoveOldestAsync(Guid trainerId, Guid gameId, int keep, CancellationToken cancellationToken = default)
    {
        IQueryable<SaveFile> backups = context.SaveFiles.Where(s => s.TrainerId == trainerId && s.GameId == gameId);

        // Runs straight against the database, after the new backup is saved.
        await backups
            .Where(s => !backups.OrderByDescending(b => b.UploadedOnUtc).Take(keep).Contains(s))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
