namespace PokeHub.Modules.Pokedex.Presentation.Me.Contracts;

// Bound from the route: me/pokemon/{pokemonId}/...
internal sealed class PokemonRouteRequest
{
    public Guid PokemonId { get; init; }
}
