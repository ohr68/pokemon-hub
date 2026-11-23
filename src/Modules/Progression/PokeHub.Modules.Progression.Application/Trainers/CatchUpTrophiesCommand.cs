using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.PublicApi;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <summary>
/// Brings every trainer's species count up to date and gives them the trophies they already qualify for. Trophies live in code, so one
/// added in a new version reaches existing trainers this way. Returns how many were awarded.
/// </summary>
public sealed record CatchUpTrophiesCommand : ICommand<int>;

internal sealed class CatchUpTrophiesCommandHandler(
    ITrainerProgressRepository repository,
    IPokedexApi pokedexApi,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CatchUpTrophiesCommand, int>
{
    public async Task<Result<int>> Handle(CatchUpTrophiesCommand request, CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;
        int awarded = 0;
        IReadOnlyDictionary<Guid, int> species = await pokedexApi.GetSpeciesEverOwnedAsync(cancellationToken: cancellationToken);
        ILookup<Guid, GameFact> facts = (await pokedexApi.GetGameFactsAsync(cancellationToken: cancellationToken)).ToLookup(f => f.TrainerId);

        foreach (TrainerProgress progress in await repository.GetAllAsync(cancellationToken))
        {
            if (species.TryGetValue(progress.TrainerId, out int count))
            {
                progress.SetSpeciesDiscovered(count, utcNow);
            }

            int before = progress.Trophies.Count;
            progress.EarnTrophies(utcNow);
            progress.EarnAchievements(TrainerAchievements.For(progress, facts[progress.TrainerId]), utcNow);
            awarded += progress.Trophies.Count - before;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return awarded;
    }
}
