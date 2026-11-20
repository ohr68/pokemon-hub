using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Presentation.Play.Docs;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed class GetPlayerSaveEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("save");
        Policies(GamesPermissions.ReadPokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces<PlayerSaveResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetPlayerSaveEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<PlayerSaveResponse> result = await sender.Send(
            new GetPlayerSaveQuery(User.GetClientId(), User.GetUserId()),
            ct);

        await SendResultAsync(result, save => Results.Ok(save));
    }
}
