using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveFiles;

namespace PokeHub.Modules.Games.Application.SaveFiles.UploadSaveFile;

internal sealed class UploadSaveFileCommandHandler(
    IGameRepository gameRepository,
    ISaveFileRepository saveFileRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UploadSaveFileCommand, UploadedSaveFileResponse>
{
    public async Task<Result<UploadedSaveFileResponse>> Handle(UploadSaveFileCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure<UploadedSaveFileResponse>(game.Error);
        }

        // Syncing again without playing sends the same save; it doesn't need another backup.
        SaveFile? latest = await saveFileRepository.GetLatestAsync(request.TrainerId, game.Value.Id, cancellationToken);

        if (latest is not null && latest.Sha256 == SaveFile.HashOf(request.Content))
        {
            return new UploadedSaveFileResponse(latest.Id, latest.UploadedOnUtc, IsNew: false);
        }

        var saveFile = SaveFile.Upload(request.TrainerId, game.Value.Id, request.Content, dateTimeProvider.UtcNow, request.Label);

        saveFileRepository.Insert(saveFile);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await saveFileRepository.RemoveOldestAsync(request.TrainerId, game.Value.Id, SaveFile.BackupsKept, cancellationToken);

        return new UploadedSaveFileResponse(saveFile.Id, saveFile.UploadedOnUtc, IsNew: true);
    }
}
