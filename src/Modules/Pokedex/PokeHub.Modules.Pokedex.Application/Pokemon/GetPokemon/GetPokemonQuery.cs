using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetPokemon;

public sealed record GetPokemonQuery(Guid PokemonId) : IQuery<PokemonDetailsResponse>;
