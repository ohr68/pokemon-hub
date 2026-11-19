using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Users.Application.Users.UpdateUser;
using PokeHub.Modules.Users.Presentation.Users.Contracts;
using PokeHub.Modules.Users.Presentation.Users.Docs;

namespace PokeHub.Modules.Users.Presentation.Users.Endpoints;

internal sealed class UpdateUserProfileEndpoint(ISender sender) : EndpointBase<UpdateUserProfileRequest>
{
    public override void Configure()
    {
        Put("profile");
        Policies(UserPermissions.ModifyUser);
        Group<UserEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new UpdateUserProfileEndpointSummary());
    }

    public override async Task HandleAsync(UpdateUserProfileRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new UpdateUserCommand(User.GetUserId(), req.FirstName, req.LastName),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
