namespace PokeHub.Modules.Progression.Domain.Trainers;

public interface ITrainerProgressRepository
{
    Task<TrainerProgress?> GetAsync(Guid trainerId, CancellationToken cancellationToken = default);

    // The sources among these the trainer already earned XP for.
    Task<IReadOnlySet<string>> GetEarnedSourcesAsync(Guid trainerId, IReadOnlyCollection<string> sources, CancellationToken cancellationToken = default);

    void Insert(TrainerProgress progress);

    void InsertEntries(IEnumerable<XpEntry> entries);
}
