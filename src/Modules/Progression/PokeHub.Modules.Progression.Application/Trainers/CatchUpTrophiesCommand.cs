using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Application.Abstractions.Data;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <summary>
/// Gives every trainer the trophies they already qualify for. Trophies live in code, so one
/// added in a new version reaches existing trainers this way. Returns how many were awarded.
/// </summary>
public sealed record CatchUpTrophiesCommand : ICommand<int>;

internal sealed class CatchUpTrophiesCommandHandler(
    ITrainerProgressRepository repository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CatchUpTrophiesCommand, int>
{
    public async Task<Result<int>> Handle(CatchUpTrophiesCommand request, CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;
        int awarded = 0;

        foreach (TrainerProgress progress in await repository.GetAllAsync(cancellationToken))
        {
            int before = progress.Trophies.Count;
            progress.EarnTrophies(utcNow);
            awarded += progress.Trophies.Count - before;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return awarded;
    }
}
