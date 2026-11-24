using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Play;

/// <param name="CatchKey">The game's own id for the catch; sending it again returns the same Pokémon.</param>
/// <param name="FromOtherGame">The save says another game caught it. Then it is only recorded if PokeHub already put it in this game, so a save from another game can't pay rewards as new catches.</param>
public sealed record CatchPokemonInGameCommand(
    string? ClientId,
    Guid TrainerId,
    int SpeciesId,
    int Level,
    bool IsShiny,
    string CatchKey,
    string? Nickname = null,
    bool FromOtherGame = false) : ICommand<Guid>;

internal sealed class CatchPokemonInGameCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<CatchPokemonInGameCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CatchPokemonInGameCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure<Guid>(game.Error);
        }

        return await pokedexApi.CatchPokemonAsync(
            new CatchRequest(
                request.TrainerId,
                game.Value.Id,
                game.Value.Name,
                request.SpeciesId,
                request.Level,
                request.IsShiny,
                request.CatchKey,
                request.Nickname,
                request.FromOtherGame),
            cancellationToken);
    }
}
