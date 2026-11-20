namespace PokeHub.Modules.Pokedex.Domain.Saves;

public interface ISaveRepository
{
    Task<Save?> GetAsync(Guid saveId, CancellationToken cancellationToken = default);

    Task<Save?> GetForGameAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default);

    void Insert(Save save);
}
