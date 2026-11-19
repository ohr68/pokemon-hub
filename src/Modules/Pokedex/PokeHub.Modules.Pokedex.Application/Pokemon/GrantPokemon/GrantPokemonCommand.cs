using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GrantPokemon;

/// <summary>
/// Gives a trainer a Pokémon directly. An admin tool until games can give Pokémon themselves.
/// </summary>
public sealed record GrantPokemonCommand(Guid TrainerId, int SpeciesId, int Level, bool IsShiny) : ICommand<Guid>;
