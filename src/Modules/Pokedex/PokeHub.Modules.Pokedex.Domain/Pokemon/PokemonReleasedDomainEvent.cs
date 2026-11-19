using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public sealed class PokemonReleasedDomainEvent(Guid pokemonId, Guid trainerId, int speciesId) : DomainEvent
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int SpeciesId { get; init; } = speciesId;
}
