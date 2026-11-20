using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.PublicApi;
using PokeHub.Modules.Trades.Application.Abstractions.Data;
using PokeHub.Modules.Trades.Domain.Trades;

namespace PokeHub.Modules.Trades.Application.Trades.ProposeTrade;

internal sealed class ProposeTradeCommandHandler(
    IPokedexApi pokedexApi,
    ITradeRepository tradeRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ProposeTradeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ProposeTradeCommand request, CancellationToken cancellationToken)
    {
        TrainerInfo? proposer = await pokedexApi.GetTrainerAsync(request.ProposerId, cancellationToken);
        TrainerInfo? recipient = await pokedexApi.GetTrainerAsync(request.RecipientId, cancellationToken);

        if (proposer is null)
        {
            return Result.Failure<Guid>(TradeErrors.TrainerNotFound(request.ProposerId));
        }

        if (recipient is null)
        {
            return Result.Failure<Guid>(TradeErrors.TrainerNotFound(request.RecipientId));
        }

        // Ownership is checked again by the Pokédex when the trade is accepted; this only
        // stops offers that could never work.
        PokemonInfo? offered = await GetAvailableAsync(request.OfferedPokemonId, proposer.Id, cancellationToken);
        PokemonInfo? requested = await GetAvailableAsync(request.RequestedPokemonId, recipient.Id, cancellationToken);

        if (offered is null)
        {
            return Result.Failure<Guid>(TradeErrors.PokemonNotAvailable(request.OfferedPokemonId));
        }

        if (requested is null)
        {
            return Result.Failure<Guid>(TradeErrors.PokemonNotAvailable(request.RequestedPokemonId));
        }

        Result<Trade> trade = Trade.Propose(
            proposer.Id,
            proposer.Name,
            recipient.Id,
            recipient.Name,
            ToSnapshot(offered),
            ToSnapshot(requested),
            dateTimeProvider.UtcNow);

        if (trade.IsFailure)
        {
            return Result.Failure<Guid>(trade.Error);
        }

        tradeRepository.Insert(trade.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return trade.Value.Id;
    }

    private async Task<PokemonInfo?> GetAvailableAsync(Guid pokemonId, Guid trainerId, CancellationToken cancellationToken)
    {
        PokemonInfo? pokemon = await pokedexApi.GetPokemonAsync(pokemonId, cancellationToken);

        // Trades happen from the hub: a Pokémon inside a game save has to be moved out first.
        return pokemon is { IsActive: true, IsInHub: true } && pokemon.TrainerId == trainerId ? pokemon : null;
    }

    private static TradedPokemon ToSnapshot(PokemonInfo pokemon)
    {
        return new TradedPokemon(
            pokemon.Id,
            pokemon.SpeciesId,
            pokemon.SpeciesName,
            pokemon.Nickname,
            pokemon.Level,
            pokemon.IsShiny);
    }
}
