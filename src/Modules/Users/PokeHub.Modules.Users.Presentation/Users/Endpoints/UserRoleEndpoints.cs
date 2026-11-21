using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Users.Application.Users.Roles;

namespace PokeHub.Modules.Users.Presentation.Users.Endpoints;

internal sealed class UserRouteRequest
{
    public Guid UserId { get; init; }
}

internal sealed class GetUsersWithRolesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("manage");
        Policies(UserPermissions.ManageRoles);
        Group<UserEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<UserWithRolesResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "Lists every user and whether they are an administrator");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<UserWithRolesResponse>> result = await sender.Send(new GetUsersWithRolesQuery(), ct);

        await SendResultAsync(result, users => Results.Ok(users));
    }
}

internal sealed class GrantAdministratorEndpoint(ISender sender) : EndpointBase<UserRouteRequest>
{
    public override void Configure()
    {
        Put("{userId:guid}/administrator");
        Policies(UserPermissions.ManageRoles);
        Group<UserEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s => s.Summary = "Makes a user an administrator");
    }

    public override async Task HandleAsync(UserRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new SetAdministratorCommand(User.GetUserId(), req.UserId, IsAdministrator: true), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class RevokeAdministratorEndpoint(ISender sender) : EndpointBase<UserRouteRequest>
{
    public override void Configure()
    {
        Delete("{userId:guid}/administrator");
        Policies(UserPermissions.ManageRoles);
        Group<UserEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Removes a user's administrator role";
            s.Description = "Nobody can remove their own role, so there is always an administrator left.";
        });
    }

    public override async Task HandleAsync(UserRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new SetAdministratorCommand(User.GetUserId(), req.UserId, IsAdministrator: false), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
