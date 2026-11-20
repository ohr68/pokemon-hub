using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Games.Docs;

internal sealed class GetGamesEndpointSummary : EndpointSummary
{
    public GetGamesEndpointSummary()
    {
        Summary = "Lists the games";
        Description = "Returns the active games players can open. Open to everyone.";
    }
}
