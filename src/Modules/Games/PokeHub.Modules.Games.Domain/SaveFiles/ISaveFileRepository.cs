namespace PokeHub.Modules.Games.Domain.SaveFiles;

public interface ISaveFileRepository
{
    Task<SaveFile?> GetLatestAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default);

    void Insert(SaveFile saveFile);

    /// <summary>Removes the player's oldest backups for the game, keeping the newest <paramref name="keep"/>.</summary>
    Task RemoveOldestAsync(Guid trainerId, Guid gameId, int keep, CancellationToken cancellationToken = default);
}
