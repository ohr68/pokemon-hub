using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.RedeliverPokemon;

/// <summary>
/// Puts Pokémon PokeHub has inside a game, but that its save no longer holds, back to waiting
/// for delivery. Nothing changes if any of them is still in the save, which would duplicate it.
/// </summary>
/// <param name="PresentCatchKeys">The catch keys of every Pokémon in the game's save now.</param>
public sealed record RedeliverPokemonCommand(
    Guid TrainerId,
    Guid GameId,
    IReadOnlyCollection<Guid> PokemonIds,
    IReadOnlyCollection<string> PresentCatchKeys) : ICommand;
