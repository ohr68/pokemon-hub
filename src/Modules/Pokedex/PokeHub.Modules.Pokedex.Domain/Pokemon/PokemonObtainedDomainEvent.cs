using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public sealed class PokemonObtainedDomainEvent(
    Guid pokemonId,
    Guid trainerId,
    int speciesId,
    PokemonOrigin origin,
    bool isShiny,
    Guid? gameId = null) : DomainEvent
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int SpeciesId { get; init; } = speciesId;

    public PokemonOrigin Origin { get; init; } = origin;

    public bool IsShiny { get; init; } = isShiny;

    // The game it was caught in, if any.
    public Guid? GameId { get; init; } = gameId;
}
