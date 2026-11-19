using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Users.Application.Users.GetUser;
using PokeHub.Modules.Users.Presentation.Users.Docs;

namespace PokeHub.Modules.Users.Presentation.Users.Endpoints;

internal sealed class GetUserProfileEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("profile");
        Policies(UserPermissions.GetUser);
        Group<UserEndpointGroup>();

        Description(builder => builder.Produces<UserResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetUserProfileEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<UserResponse> result = await sender.Send(new GetUserQuery(User.GetUserId()), ct);

        await SendResultAsync(result, user => Results.Ok(user));
    }
}
