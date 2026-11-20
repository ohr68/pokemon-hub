using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.Games.RegisterGame;

internal sealed class RegisterGameCommandHandler(
    IGameRepository gameRepository,
    IGameClientService gameClientService,
    IGameOriginCache gameOriginCache,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterGameCommand, RegisteredGameResponse>
{
    public async Task<Result<RegisteredGameResponse>> Handle(RegisterGameCommand request, CancellationToken cancellationToken)
    {
        if (await gameRepository.SlugExistsAsync(request.Slug, cancellationToken))
        {
            return Result.Failure<RegisteredGameResponse>(GameErrors.SlugTaken(request.Slug));
        }

        var game = Game.Register(
            request.Name,
            request.Slug,
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            request.Url,
            [.. request.WebOrigins],
            dateTimeProvider.UtcNow);

        // The sign-in client comes first: a game saved without one would be unusable.
        Result client = await gameClientService.CreateClientAsync(
            new GameClient(game.ClientId, game.Name, request.RedirectUris, request.WebOrigins),
            cancellationToken);

        if (client.IsFailure)
        {
            return Result.Failure<RegisteredGameResponse>(client.Error);
        }

        gameRepository.Insert(game);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        gameOriginCache.Invalidate();

        return new RegisteredGameResponse(game.Id, game.ClientId);
    }
}
