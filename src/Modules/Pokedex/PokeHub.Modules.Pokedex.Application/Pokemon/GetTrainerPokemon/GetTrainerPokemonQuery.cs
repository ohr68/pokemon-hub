using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;

/// <summary>
/// The Pokémon a trainer currently has.
/// </summary>
/// <param name="HubOnly">Only Pokémon in the hub (the ones that can be traded).</param>
/// <param name="GameId">Only Pokémon in the trainer's save for this game.</param>
public sealed record GetTrainerPokemonQuery(Guid TrainerId, bool HubOnly = false, Guid? GameId = null)
    : IQuery<IReadOnlyCollection<PokemonResponse>>;
