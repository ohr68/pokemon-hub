using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.RenamePokemon;

/// <param name="Nickname">An empty nickname removes it.</param>
public sealed record RenamePokemonCommand(Guid TrainerId, Guid PokemonId, string? Nickname) : ICommand;
