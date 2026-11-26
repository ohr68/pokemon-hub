namespace PokeHub.Modules.Games.Domain.SaveEdits;

public interface ISaveEditRepository
{
    /// <summary>Edits of this trainer's save for the game that a later save hasn't settled yet.</summary>
    Task<IReadOnlyList<SaveEdit>> GetOpenAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default);

    Task<SaveEdit?> GetAsync(Guid id, Guid trainerId, CancellationToken cancellationToken = default);

    void Insert(SaveEdit saveEdit);
}
