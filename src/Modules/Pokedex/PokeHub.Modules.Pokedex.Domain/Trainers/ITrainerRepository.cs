namespace PokeHub.Modules.Pokedex.Domain.Trainers;

public interface ITrainerRepository
{
    Task<Trainer?> GetAsync(Guid trainerId, CancellationToken cancellationToken = default);

    void Insert(Trainer trainer);
}
