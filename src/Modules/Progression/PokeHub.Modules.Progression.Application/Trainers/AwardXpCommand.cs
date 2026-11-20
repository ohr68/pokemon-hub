using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Application.Abstractions.Data;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <param name="Source">What earned it (pokemon-obtained:{id}, trade:{id}, ...); each pays once.</param>
/// <param name="EarnedOnUtc">When it happened.</param>
public sealed record XpAward(XpReason Reason, string Source, DateTime EarnedOnUtc);

public sealed record AwardXpCommand(Guid TrainerId, IReadOnlyCollection<XpAward> Awards) : ICommand;

internal sealed class AwardXpCommandHandler(
    ITrainerProgressRepository repository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AwardXpCommand>
{
    public async Task<Result> Handle(AwardXpCommand request, CancellationToken cancellationToken)
    {
        IReadOnlySet<string> earned = await repository.GetEarnedSourcesAsync(
            request.TrainerId,
            request.Awards.Select(a => a.Source).ToList(),
            cancellationToken);

        var awards = request.Awards.Where(a => !earned.Contains(a.Source)).ToList();

        if (awards.Count == 0)
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

        foreach (XpAward award in awards)
        {
            progress.Earn(award.Reason, award.Source, award.EarnedOnUtc, utcNow);
        }

        repository.InsertEntries(progress.NewEntries);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
