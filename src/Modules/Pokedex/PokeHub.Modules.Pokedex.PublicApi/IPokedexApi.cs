using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>
/// What other modules may ask of the Pokédex, in process. The Pokédex stays the only owner
/// of Pokémon: other modules read through here and ask it to make changes.
/// </summary>
public interface IPokedexApi
{
    Task<PokemonInfo?> GetPokemonAsync(Guid pokemonId, CancellationToken cancellationToken = default);

    Task<TrainerInfo?> GetTrainerAsync(Guid trainerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Swaps two Pokémon between their trainers in one transaction, evolving those that evolve
    /// by trading. Safe to repeat for the same trade.
    /// </summary>
    Task<Result> TradePokemonAsync(TradeRequest request, CancellationToken cancellationToken = default);
}
