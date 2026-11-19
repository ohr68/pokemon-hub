using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.TradePokemon;

internal sealed class TradePokemonCommandHandler(
    IOwnedPokemonRepository pokemonRepository,
    ISpeciesRepository speciesRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TradePokemonCommand>
{
    public async Task<Result> Handle(TradePokemonCommand request, CancellationToken cancellationToken)
    {
        // A retried accept (for example after the Trades module failed to save) must not swap back.
        if (await pokemonRepository.IsTradeCompletedAsync(request.TradeId, cancellationToken))
        {
            return Result.Success();
        }

        OwnedPokemon? first = await pokemonRepository.GetAsync(request.FirstPokemonId, cancellationToken);
        OwnedPokemon? second = await pokemonRepository.GetAsync(request.SecondPokemonId, cancellationToken);

        if (first is null)
        {
            return Result.Failure(PokemonErrors.NotFound(request.FirstPokemonId));
        }

        if (second is null)
        {
            return Result.Failure(PokemonErrors.NotFound(request.SecondPokemonId));
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        Result firstTrade = first.TradeTo(
            request.FirstTrainerId,
            request.SecondTrainerId,
            request.TradeId,
            await GetTradeEvolutionAsync(first.SpeciesId, cancellationToken),
            utcNow);

        if (firstTrade.IsFailure)
        {
            return firstTrade;
        }

        Result secondTrade = second.TradeTo(
            request.SecondTrainerId,
            request.FirstTrainerId,
            request.TradeId,
            await GetTradeEvolutionAsync(second.SpeciesId, cancellationToken),
            utcNow);

        if (secondTrade.IsFailure)
        {
            return secondTrade;
        }

        // Both Pokémon carry a row version: if either changed since it was read (another trade,
        // a release), saving fails as a whole and neither moves.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<int?> GetTradeEvolutionAsync(int speciesId, CancellationToken cancellationToken)
    {
        PokemonSpecies? species = await speciesRepository.GetAsync(speciesId, cancellationToken);

        return species?.TradeEvolutionSpeciesId;
    }
}
