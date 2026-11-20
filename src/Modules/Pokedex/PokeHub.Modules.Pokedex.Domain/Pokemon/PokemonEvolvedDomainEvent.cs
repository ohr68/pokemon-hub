using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

/// <summary>The Pokémon evolved: by trading, or in a game (found when it came back to the hub).</summary>
public sealed class PokemonEvolvedDomainEvent(
    Guid pokemonId,
    Guid trainerId,
    int fromSpeciesId,
    int toSpeciesId) : DomainEvent
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int FromSpeciesId { get; init; } = fromSpeciesId;

    public int ToSpeciesId { get; init; } = toSpeciesId;
}
