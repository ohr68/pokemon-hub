using FastEndpoints;

namespace PokeHub.Modules.Users.Presentation.Users.Docs;

internal sealed class UpdateUserProfileEndpointSummary : EndpointSummary
{
    public UpdateUserProfileEndpointSummary()
    {
        Summary = "Updates the current user's profile";
        Description = "Changes the first and last name of the user identified by the access token.";
    }
}
