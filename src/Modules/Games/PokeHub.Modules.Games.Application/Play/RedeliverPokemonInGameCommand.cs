using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Play;

/// <param name="PresentCatchKeys">The catch keys of every Pokémon in the game's save now.</param>
public sealed record RedeliverPokemonInGameCommand(
    string? ClientId,
    Guid TrainerId,
    IReadOnlyCollection<Guid> PokemonIds,
    IReadOnlyCollection<string> PresentCatchKeys) : ICommand;

internal sealed class RedeliverPokemonInGameCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<RedeliverPokemonInGameCommand>
{
    public async Task<Result> Handle(RedeliverPokemonInGameCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure(game.Error);
        }

        return await pokedexApi.RedeliverAsync(
            request.TrainerId,
            game.Value.Id,
            request.PokemonIds,
            request.PresentCatchKeys,
            cancellationToken);
    }
}
