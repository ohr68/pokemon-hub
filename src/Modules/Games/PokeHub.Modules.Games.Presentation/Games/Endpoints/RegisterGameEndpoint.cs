using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Games.RegisterGame;
using PokeHub.Modules.Games.Presentation.Games.Contracts;
using PokeHub.Modules.Games.Presentation.Games.Docs;

namespace PokeHub.Modules.Games.Presentation.Games.Endpoints;

internal sealed class RegisterGameEndpoint(ISender sender) : EndpointBase<RegisterGameRequest>
{
    public override void Configure()
    {
        Post("/");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<RegisteredGameResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new RegisterGameEndpointSummary());
    }

    public override async Task HandleAsync(RegisterGameRequest req, CancellationToken ct)
    {
        Result<RegisteredGameResponse> result = await sender.Send(
            new RegisterGameCommand(req.Name, req.Slug, req.Url, req.RedirectUris, req.WebOrigins),
            ct);

        await SendResultAsync(result, game => Results.Ok(game));
    }
}
