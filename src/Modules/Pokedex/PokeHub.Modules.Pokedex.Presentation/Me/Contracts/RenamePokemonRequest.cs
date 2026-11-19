namespace PokeHub.Modules.Pokedex.Presentation.Me.Contracts;

internal sealed class RenamePokemonRequest
{
    // From the route.
    public Guid PokemonId { get; init; }

    // From the body.
    public string? Nickname { get; init; }
}
