namespace PokeHub.Modules.Pokedex.Presentation.Trainers.Contracts;

internal sealed class GrantPokemonRequest
{
    // From the route.
    public Guid TrainerId { get; init; }

    public int SpeciesId { get; init; }

    public int Level { get; init; } = 5;

    public bool IsShiny { get; init; }
}
