using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Pokedex.IntegrationEvents;

/// <summary>A trainer got a new Pokémon: their starter, a catch in a game, or a grant.</summary>
/// <param name="origin">starter, game or grant.</param>
/// <param name="isNewSpecies">The trainer never had this species before.</param>
public sealed class PokemonObtainedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid pokemonId,
    Guid trainerId,
    int speciesId,
    string origin,
    bool isShiny,
    bool isNewSpecies) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int SpeciesId { get; init; } = speciesId;

    public string Origin { get; init; } = origin;

    public bool IsShiny { get; init; } = isShiny;

    public bool IsNewSpecies { get; init; } = isNewSpecies;
}
