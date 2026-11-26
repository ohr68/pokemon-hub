using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.PublicApi;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.SaveEdits.GetWaiting;

/// <summary>
/// What PokeHub has waiting for the game: Pokémon moved into its save on PokeHub and shop items
/// bought for it, so a device can say what a send will bring in before it sends.
/// </summary>
/// <param name="ClientId">The client the player signed in with, from the token.</param>
public sealed record GetWaitingQuery(string? ClientId, Guid TrainerId) : IQuery<WaitingResponse>;

/// <param name="Editable">Whether PokeHub can put them into the save a device sends (save/edits).</param>
public sealed record WaitingResponse(
    bool Editable,
    IReadOnlyCollection<WaitingPokemonResponse> Pokemon,
    IReadOnlyCollection<WaitingItemResponse> Items);

/// <param name="Name">As the game shows it: its nickname, or its species in capitals.</param>
public sealed record WaitingPokemonResponse(Guid Id, int SpeciesId, string Name, int Level, bool IsShiny);

public sealed record WaitingItemResponse(Guid PurchaseId, string Name, int Quantity);

internal sealed class GetWaitingQueryHandler(IGameRepository gameRepository, IPokedexApi pokedexApi, IEconomyApi economyApi)
    : IQueryHandler<GetWaitingQuery, WaitingResponse>
{
    public async Task<Result<WaitingResponse>> Handle(GetWaitingQuery request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);
        if (game.IsFailure)
        {
            return Result.Failure<WaitingResponse>(game.Error);
        }

        Result<IReadOnlyCollection<DeliveryInfo>> pokemon =
            await pokedexApi.GetAwaitingDeliveryAsync(request.TrainerId, game.Value.Id, cancellationToken);
        if (pokemon.IsFailure)
        {
            return Result.Failure<WaitingResponse>(pokemon.Error);
        }

        IReadOnlyCollection<AwaitingItem> items = await economyApi.GetAwaitingItemsAsync(request.TrainerId, game.Value.Id, cancellationToken);

        return new WaitingResponse(
            Gen3Game.ForSlug(game.Value.Slug) is not null,
            [.. pokemon.Value.Select(p => new WaitingPokemonResponse(
                p.Id, p.SpeciesId, p.Nickname ?? Gen3NewPokemon.DisplayName(p.SpeciesName), p.Level, p.IsShiny))],
            [.. items.Select(i => new WaitingItemResponse(i.PurchaseId, i.ItemName, i.Quantity))]);
    }
}
