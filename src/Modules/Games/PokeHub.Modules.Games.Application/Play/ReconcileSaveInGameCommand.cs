using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Play;

/// <param name="PresentCatchKeys">The catch keys of every Pokémon in the game's save now.</param>
/// <param name="DryRun">Only report which Pokémon would be removed.</param>
public sealed record ReconcileSaveInGameCommand(
    string? ClientId,
    Guid TrainerId,
    IReadOnlyCollection<string> PresentCatchKeys,
    bool DryRun) : ICommand<IReadOnlyCollection<LeftGameInfo>>;

internal sealed class ReconcileSaveInGameCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<ReconcileSaveInGameCommand, IReadOnlyCollection<LeftGameInfo>>
{
    public async Task<Result<IReadOnlyCollection<LeftGameInfo>>> Handle(
        ReconcileSaveInGameCommand request,
        CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<LeftGameInfo>>(game.Error);
        }

        return await pokedexApi.ReconcileSaveAsync(
            request.TrainerId,
            game.Value.Id,
            request.PresentCatchKeys,
            request.DryRun,
            cancellationToken);
    }
}
