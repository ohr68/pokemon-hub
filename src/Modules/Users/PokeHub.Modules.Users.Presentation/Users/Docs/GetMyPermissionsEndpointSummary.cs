using FastEndpoints;

namespace PokeHub.Modules.Users.Presentation.Users.Docs;

internal sealed class GetMyPermissionsEndpointSummary : EndpointSummary
{
    public GetMyPermissionsEndpointSummary()
    {
        Summary = "Lists the signed-in user's permissions";
        Description = "Lets a frontend show what the user may do, such as the admin pages (games:manage). The API still checks every request.";
    }
}
