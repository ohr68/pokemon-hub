using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class GetMyPokemonEndpointSummary : EndpointSummary
{
    public GetMyPokemonEndpointSummary()
    {
        Summary = "Lists my Pokémon";
        Description = "Returns the Pokémon the current trainer has now.";
    }
}
