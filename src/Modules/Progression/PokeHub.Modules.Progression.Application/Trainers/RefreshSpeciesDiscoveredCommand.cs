using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.PublicApi;
using PokeHub.Modules.Pokedex.PublicApi;
using PokeHub.Modules.Progression.Application.Abstractions.Data;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <summary>Sets the trainer's species count from the Pokédex and awards any trophy now reached, general, per game or cross-game.</summary>
public sealed record RefreshSpeciesDiscoveredCommand(Guid TrainerId) : ICommand;

internal sealed class RefreshSpeciesDiscoveredCommandHandler(
    ITrainerProgressRepository repository,
    IPokedexApi pokedexApi,
    IPlayerGameCounters gameCounters,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshSpeciesDiscoveredCommand>
{
    public async Task<Result> Handle(RefreshSpeciesDiscoveredCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<Guid, int> species = await pokedexApi.GetSpeciesEverOwnedAsync(request.TrainerId, cancellationToken);

        if (!species.TryGetValue(request.TrainerId, out int count))
        {
            return Result.Success();
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        TrainerProgress? progress = await repository.GetAsync(request.TrainerId, cancellationToken);

        if (progress is null)
        {
            progress = TrainerProgress.Start(request.TrainerId, utcNow);
            repository.Insert(progress);
        }

        progress.SetSpeciesDiscovered(count, utcNow);
        progress.EarnTrophies(utcNow);

        IReadOnlyCollection<GameFact> facts = await pokedexApi.GetGameFactsAsync(request.TrainerId, cancellationToken);
        IReadOnlyCollection<GameDexProgress> dexes = await pokedexApi.GetGameDexProgressAsync(request.TrainerId, cancellationToken);
        AchievementProgress achievements = TrainerAchievements.For(progress, facts, dexes);
        progress.EarnAchievements(achievements, utcNow);

        SpecificSources sources = await TrainerAchievements.LoadSourcesAsync(pokedexApi, gameCounters, request.TrainerId, cancellationToken);
        progress.EarnSpecific(TrainerAchievements.Specific(request.TrainerId, facts, achievements, sources), utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
