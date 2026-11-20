using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.MovePokemon;

/// <param name="SaveId">The save to move into; empty moves the Pokémon back to the hub.</param>
public sealed record MovePokemonCommand(Guid TrainerId, Guid PokemonId, Guid? SaveId) : ICommand;
