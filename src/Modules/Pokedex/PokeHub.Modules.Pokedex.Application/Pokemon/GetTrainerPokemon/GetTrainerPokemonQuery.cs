using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;

/// <summary>
/// The Pokémon a trainer currently has.
/// </summary>
public sealed record GetTrainerPokemonQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<PokemonResponse>>;
