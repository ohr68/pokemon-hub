using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.ReleasePokemon;

/// <param name="GameId">The game releasing it from its save; empty releases it from the hub.</param>
public sealed record ReleasePokemonCommand(Guid TrainerId, Guid PokemonId, Guid? GameId = null) : ICommand;
