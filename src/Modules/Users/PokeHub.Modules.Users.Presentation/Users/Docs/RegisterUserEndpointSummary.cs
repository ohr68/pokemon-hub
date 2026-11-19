using FastEndpoints;

namespace PokeHub.Modules.Users.Presentation.Users.Docs;

internal sealed class RegisterUserEndpointSummary : EndpointSummary
{
    public RegisterUserEndpointSummary()
    {
        Summary = "Registers a new user";
        Description = "Creates the user in the identity provider and in PokeHub, and returns the new user's id.";
    }
}
