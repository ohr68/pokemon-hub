using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Pokedex.IntegrationEvents;

/// <summary>A trainer got a new Pokémon: their starter, a catch in a game, or a grant.</summary>
/// <param name="origin">starter, game, grant or shop.</param>
/// <param name="isNewSpecies">The trainer never had this species before.</param>
/// <param name="isRare">The species is legendary or mythical.</param>
/// <param name="gameId">The game it was caught in, if any.</param>
public sealed class PokemonObtainedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid pokemonId,
    Guid trainerId,
    int speciesId,
    string origin,
    bool isShiny,
    bool isNewSpecies,
    bool isRare,
    Guid? gameId = null) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int SpeciesId { get; init; } = speciesId;

    public string Origin { get; init; } = origin;

    public bool IsShiny { get; init; } = isShiny;

    public bool IsNewSpecies { get; init; } = isNewSpecies;

    public bool IsRare { get; init; } = isRare;

    // The game it was caught in, if any.
    public Guid? GameId { get; init; } = gameId;
}
