using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Games.Docs;

internal sealed class RegisterGameEndpointSummary : EndpointSummary
{
    public RegisterGameEndpointSummary()
    {
        Summary = "Registers a game";
        Description = "Creates the game and its Keycloak client (public, PKCE). Returns the client id the game uses to sign players in. Requires the games:manage permission.";
    }
}
