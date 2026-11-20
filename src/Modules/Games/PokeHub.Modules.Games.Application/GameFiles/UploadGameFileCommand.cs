using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Domain.GameFiles;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.GameFiles;

/// <summary>
/// Adds a file to a game. A cover, icon or ROM replaces the game's current one; screenshots add up.
/// </summary>
public sealed record UploadGameFileCommand(
    Guid GameId,
    GameFileKind Kind,
    string FileName,
    string ContentType,
    byte[] Content) : ICommand<Guid>;

internal sealed class UploadGameFileCommandHandler(
    IGameRepository gameRepository,
    IGameFileRepository fileRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UploadGameFileCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UploadGameFileCommand request, CancellationToken cancellationToken)
    {
        if (await gameRepository.GetAsync(request.GameId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(GameErrors.NotFound(request.GameId));
        }

        Result<GameFile> file = GameFile.Upload(
            request.GameId,
            request.Kind,
            request.FileName,
            request.ContentType,
            request.Content,
            dateTimeProvider.UtcNow);

        if (file.IsFailure)
        {
            return Result.Failure<Guid>(file.Error);
        }

        if (file.Value.IsSingle &&
            await fileRepository.GetSingleAsync(request.GameId, request.Kind, cancellationToken) is { } current)
        {
            fileRepository.Remove(current);
        }

        fileRepository.Insert(file.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return file.Value.Id;
    }
}
