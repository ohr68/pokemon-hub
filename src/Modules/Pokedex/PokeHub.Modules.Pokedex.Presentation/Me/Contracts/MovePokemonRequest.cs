namespace PokeHub.Modules.Pokedex.Presentation.Me.Contracts;

internal sealed class MovePokemonRequest
{
    // From the route.
    public Guid PokemonId { get; init; }

    // From the body: the save to move into, or empty to bring it back to the hub.
    public Guid? SaveId { get; init; }
}
