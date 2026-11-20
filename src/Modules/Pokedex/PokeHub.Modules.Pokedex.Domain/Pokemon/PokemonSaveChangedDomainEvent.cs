using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public sealed class PokemonSaveChangedDomainEvent(
    Guid pokemonId,
    Guid trainerId,
    int speciesId,
    Guid saveId,
    SaveChange change) : DomainEvent
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int SpeciesId { get; init; } = speciesId;

    public Guid SaveId { get; init; } = saveId;

    public SaveChange Change { get; init; } = change;
}
