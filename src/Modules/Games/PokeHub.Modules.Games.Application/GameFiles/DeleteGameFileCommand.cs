using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Domain.GameFiles;

namespace PokeHub.Modules.Games.Application.GameFiles;

public sealed record DeleteGameFileCommand(Guid GameId, Guid FileId) : ICommand;

internal sealed class DeleteGameFileCommandHandler(IGameFileRepository fileRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteGameFileCommand>
{
    public async Task<Result> Handle(DeleteGameFileCommand request, CancellationToken cancellationToken)
    {
        GameFile? file = await fileRepository.GetAsync(request.FileId, cancellationToken);

        if (file is null || file.GameId != request.GameId)
        {
            return Result.Failure(GameFileErrors.NotFound(request.FileId));
        }

        fileRepository.Remove(file);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
