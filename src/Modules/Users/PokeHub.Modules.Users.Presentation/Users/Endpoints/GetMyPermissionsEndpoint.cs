using Microsoft.AspNetCore.Http;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Users.Presentation.Users.Docs;

namespace PokeHub.Modules.Users.Presentation.Users.Endpoints;

internal sealed class GetMyPermissionsEndpoint : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("permissions");
        Policies(UserPermissions.GetUser);
        Group<UserEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<string>>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetMyPermissionsEndpointSummary());
    }

    // The permissions were already loaded for this request (CustomClaimsTransformation).
    public override Task HandleAsync(CancellationToken ct) =>
        Send.ResultAsync(Results.Ok(User.FindAll(CustomClaims.Permission).Select(c => c.Value).Order().ToList()));
}
