using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Play;

/// <param name="CatchKeys">The withdrawn Pokémon the game's save still holds.</param>
/// <param name="PresentCatchKeys">The catch keys of every Pokémon in the game's save now.</param>
public sealed record CancelWithdrawalInGameCommand(
    string? ClientId,
    Guid TrainerId,
    IReadOnlyCollection<string> CatchKeys,
    IReadOnlyCollection<string> PresentCatchKeys) : ICommand;

internal sealed class CancelWithdrawalInGameCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<CancelWithdrawalInGameCommand>
{
    public async Task<Result> Handle(CancelWithdrawalInGameCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure(game.Error);
        }

        return await pokedexApi.CancelWithdrawalAsync(
            request.TrainerId,
            game.Value.Id,
            request.CatchKeys,
            request.PresentCatchKeys,
            cancellationToken);
    }
}
