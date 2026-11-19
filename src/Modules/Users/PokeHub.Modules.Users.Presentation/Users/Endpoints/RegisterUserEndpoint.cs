using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Users.Application.Users.RegisterUser;
using PokeHub.Modules.Users.Presentation.Users.Contracts;
using PokeHub.Modules.Users.Presentation.Users.Docs;

namespace PokeHub.Modules.Users.Presentation.Users.Endpoints;

internal sealed class RegisterUserEndpoint(ISender sender) : EndpointBase<RegisterUserRequest>
{
    public override void Configure()
    {
        Post("register");
        AllowAnonymous();
        Group<UserEndpointGroup>();

        Description(builder => builder.Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(new RegisterUserEndpointSummary());
    }

    public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(
            new RegisterUserCommand(req.Email, req.Password, req.FirstName, req.LastName),
            ct);

        await SendResultAsync(result, userId => Results.Ok(userId));
    }
}
