using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Pokedex.IntegrationEvents;

/// <summary>A trainer's Pokémon evolved, by trading or in a game.</summary>
public sealed class PokemonEvolvedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid pokemonId,
    Guid trainerId,
    int fromSpeciesId,
    int toSpeciesId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int FromSpeciesId { get; init; } = fromSpeciesId;

    public int ToSpeciesId { get; init; } = toSpeciesId;
}
