using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.DeliverPokemon;

/// <summary>
/// The game wrote Pokémon waiting for it into its own save. All of them are recorded, or none.
/// </summary>
public sealed record DeliverPokemonCommand(Guid TrainerId, Guid GameId, IReadOnlyCollection<Delivery> Deliveries)
    : ICommand;

/// <param name="CatchKey">How the game knows the Pokémon now.</param>
public sealed record Delivery(Guid PokemonId, string CatchKey);
