namespace PokeHub.Modules.Pokedex.Presentation.Me.Contracts;

/// <param name="PokemonId">One of the trainer's Pokémon, or empty to remove the picture.</param>
internal sealed record SetAvatarRequest(Guid? PokemonId);
