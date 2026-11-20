using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Play;

/// <param name="ClientId">The client the player signed in with, from the token.</param>
public sealed record GetPlayerSaveQuery(string? ClientId, Guid TrainerId) : IQuery<PlayerSaveResponse>;

public sealed record PlayerSaveResponse(
    Guid SaveId,
    Guid GameId,
    string GameName,
    IReadOnlyCollection<SavePokemonInfo> Pokemon);

internal sealed class GetPlayerSaveQueryHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : IQueryHandler<GetPlayerSaveQuery, PlayerSaveResponse>
{
    public async Task<Result<PlayerSaveResponse>> Handle(GetPlayerSaveQuery request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure<PlayerSaveResponse>(game.Error);
        }

        // Opening the game for the first time creates the player's save.
        Result<SaveInfo> save = await pokedexApi.GetOrCreateSaveAsync(
            request.TrainerId,
            game.Value.Id,
            game.Value.Name,
            cancellationToken);

        if (save.IsFailure)
        {
            return Result.Failure<PlayerSaveResponse>(save.Error);
        }

        Result<IReadOnlyCollection<SavePokemonInfo>> pokemon = await pokedexApi.GetSavePokemonAsync(
            request.TrainerId,
            game.Value.Id,
            cancellationToken);

        if (pokemon.IsFailure)
        {
            return Result.Failure<PlayerSaveResponse>(pokemon.Error);
        }

        return new PlayerSaveResponse(save.Value.Id, game.Value.Id, game.Value.Name, pokemon.Value);
    }
}
