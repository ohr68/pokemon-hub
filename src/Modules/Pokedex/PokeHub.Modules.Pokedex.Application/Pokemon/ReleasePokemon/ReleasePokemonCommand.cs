using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.ReleasePokemon;

public sealed record ReleasePokemonCommand(Guid TrainerId, Guid PokemonId) : ICommand;
