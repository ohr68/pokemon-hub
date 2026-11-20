using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Play;

public sealed record ReleasePokemonInGameCommand(string? ClientId, Guid TrainerId, Guid PokemonId) : ICommand;

internal sealed class ReleasePokemonInGameCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<ReleasePokemonInGameCommand>
{
    public async Task<Result> Handle(ReleasePokemonInGameCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return game;
        }

        // A game can only release Pokémon in its own save.
        return await pokedexApi.ReleaseFromSaveAsync(request.TrainerId, game.Value.Id, request.PokemonId, cancellationToken);
    }
}
