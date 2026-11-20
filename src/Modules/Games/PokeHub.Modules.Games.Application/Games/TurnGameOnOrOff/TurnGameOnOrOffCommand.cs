using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.Games.TurnGameOnOrOff;

/// <summary>
/// A game turned off leaves the games list, its sign-in client is disabled in the identity
/// provider, and the game API and CORS stop accepting it. Its players' Pokémon and saves stay.
/// </summary>
public sealed record TurnGameOnOrOffCommand(Guid GameId, bool On) : ICommand;

internal sealed class TurnGameOnOrOffCommandHandler(
    IGameRepository gameRepository,
    IGameClientService gameClientService,
    IGameOriginCache gameCache,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TurnGameOnOrOffCommand>
{
    public async Task<Result> Handle(TurnGameOnOrOffCommand request, CancellationToken cancellationToken)
    {
        Game? game = await gameRepository.GetAsync(request.GameId, cancellationToken);

        if (game is null)
        {
            return Result.Failure(GameErrors.NotFound(request.GameId));
        }

        if (game.IsActive == request.On)
        {
            return Result.Success();
        }

        // The sign-in client first: a game marked on with a disabled client couldn't be used.
        Result client = await gameClientService.SetEnabledAsync(game.ClientId, request.On, cancellationToken);

        if (client.IsFailure)
        {
            return client;
        }

        if (request.On)
        {
            game.TurnOn();
        }
        else
        {
            game.TurnOff();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        gameCache.Invalidate();

        return Result.Success();
    }
}
