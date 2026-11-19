using FastEndpoints;

namespace PokeHub.Modules.Users.Presentation.Users.Docs;

internal sealed class GetUserProfileEndpointSummary : EndpointSummary
{
    public GetUserProfileEndpointSummary()
    {
        Summary = "Gets the current user's profile";
        Description = "Returns the profile of the user identified by the access token.";
    }
}
