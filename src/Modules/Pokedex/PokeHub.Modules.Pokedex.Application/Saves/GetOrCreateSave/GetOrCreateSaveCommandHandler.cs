using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Saves;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Saves.GetOrCreateSave;

internal sealed class GetOrCreateSaveCommandHandler(
    ITrainerRepository trainerRepository,
    ISaveRepository saveRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GetOrCreateSaveCommand, SaveResponse>
{
    public async Task<Result<SaveResponse>> Handle(GetOrCreateSaveCommand request, CancellationToken cancellationToken)
    {
        Result<Save> save = await GetOrCreateAsync(
            trainerRepository,
            saveRepository,
            dateTimeProvider,
            request.TrainerId,
            request.GameId,
            request.GameName,
            cancellationToken);

        if (save.IsFailure)
        {
            return Result.Failure<SaveResponse>(save.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SaveResponse(save.Value.Id, save.Value.GameId, save.Value.GameName, save.Value.CreatedOnUtc, 0);
    }

    // Shared with catching: a catch in a game the trainer never opened creates the save too.
    internal static async Task<Result<Save>> GetOrCreateAsync(
        ITrainerRepository trainerRepository,
        ISaveRepository saveRepository,
        IDateTimeProvider dateTimeProvider,
        Guid trainerId,
        Guid gameId,
        string gameName,
        CancellationToken cancellationToken)
    {
        Save? save = await saveRepository.GetForGameAsync(trainerId, gameId, cancellationToken);

        if (save is not null)
        {
            return save;
        }

        if (await trainerRepository.GetAsync(trainerId, cancellationToken) is null)
        {
            return Result.Failure<Save>(TrainerErrors.NotFound(trainerId));
        }

        save = Save.Create(trainerId, gameId, gameName, dateTimeProvider.UtcNow);

        saveRepository.Insert(save);

        return save;
    }
}
