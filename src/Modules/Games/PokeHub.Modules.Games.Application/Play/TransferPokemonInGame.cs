using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Play;

// Moving Pokémon between the hub and the game's own save, like Pokémon HOME: Pokémon moved
// into the game's save on PokeHub wait until the game writes them in (deliveries), and the
// game sends Pokémon it takes out of its save to the hub (withdrawals).

/// <param name="ClientId">The client the player signed in with, from the token.</param>
public sealed record GetDeliveriesQuery(string? ClientId, Guid TrainerId) : IQuery<IReadOnlyCollection<DeliveryInfo>>;

public sealed record DeliverPokemonInGameCommand(
    string? ClientId,
    Guid TrainerId,
    IReadOnlyCollection<DeliveredPokemon> Deliveries) : ICommand;

public sealed record WithdrawPokemonInGameCommand(
    string? ClientId,
    Guid TrainerId,
    IReadOnlyCollection<WithdrawnPokemon> Withdrawals) : ICommand<IReadOnlyCollection<Guid>>;

internal sealed class GetDeliveriesQueryHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : IQueryHandler<GetDeliveriesQuery, IReadOnlyCollection<DeliveryInfo>>
{
    public async Task<Result<IReadOnlyCollection<DeliveryInfo>>> Handle(
        GetDeliveriesQuery request,
        CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        return game.IsFailure
            ? Result.Failure<IReadOnlyCollection<DeliveryInfo>>(game.Error)
            : await pokedexApi.GetAwaitingDeliveryAsync(request.TrainerId, game.Value.Id, cancellationToken);
    }
}

internal sealed class DeliverPokemonInGameCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<DeliverPokemonInGameCommand>
{
    public async Task<Result> Handle(DeliverPokemonInGameCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        return game.IsFailure
            ? Result.Failure(game.Error)
            : await pokedexApi.DeliverPokemonAsync(request.TrainerId, game.Value.Id, request.Deliveries, cancellationToken);
    }
}

internal sealed class WithdrawPokemonInGameCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<WithdrawPokemonInGameCommand, IReadOnlyCollection<Guid>>
{
    public async Task<Result<IReadOnlyCollection<Guid>>> Handle(
        WithdrawPokemonInGameCommand request,
        CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        return game.IsFailure
            ? Result.Failure<IReadOnlyCollection<Guid>>(game.Error)
            : await pokedexApi.WithdrawPokemonAsync(request.TrainerId, game.Value.Id, request.Withdrawals, cancellationToken);
    }
}
