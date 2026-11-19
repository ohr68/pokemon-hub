using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.TradePokemon;

/// <summary>
/// Swaps two Pokémon between their trainers in one transaction. Running it again for the
/// same trade succeeds without swapping twice.
/// </summary>
public sealed record TradePokemonCommand(
    Guid TradeId,
    Guid FirstTrainerId,
    Guid FirstPokemonId,
    Guid SecondTrainerId,
    Guid SecondPokemonId) : ICommand;
