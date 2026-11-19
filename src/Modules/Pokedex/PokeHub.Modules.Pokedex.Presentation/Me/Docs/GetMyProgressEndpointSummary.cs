using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class GetMyProgressEndpointSummary : EndpointSummary
{
    public GetMyProgressEndpointSummary()
    {
        Summary = "Gets my Pokédex progress";
        Description = "Returns how many species the current trainer has caught out of the whole catalog. Released Pokémon still count, like in the games.";
    }
}
