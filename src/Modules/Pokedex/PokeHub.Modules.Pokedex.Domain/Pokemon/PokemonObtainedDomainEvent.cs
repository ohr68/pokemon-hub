using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public sealed class PokemonObtainedDomainEvent(
    Guid pokemonId,
    Guid trainerId,
    int speciesId,
    PokemonOrigin origin) : DomainEvent
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int SpeciesId { get; init; } = speciesId;

    public PokemonOrigin Origin { get; init; } = origin;
}
