using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class GetMySavesEndpointSummary : EndpointSummary
{
    public GetMySavesEndpointSummary()
    {
        Summary = "Lists my game saves";
        Description = "Returns a save for every game the trainer has played, with how many Pokémon are in it.";
    }
}
