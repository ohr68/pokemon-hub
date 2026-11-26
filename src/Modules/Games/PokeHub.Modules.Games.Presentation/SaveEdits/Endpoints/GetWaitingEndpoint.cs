using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.SaveEdits.GetWaiting;
using PokeHub.Modules.Games.Presentation.Play;

namespace PokeHub.Modules.Games.Presentation.SaveEdits.Endpoints;

internal sealed class GetWaitingEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("save/waiting");
        Policies(GamesPermissions.ReadPokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces<WaitingResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "What's waiting for the game";
            s.Description = "The Pokémon moved into the player's save for this game on PokeHub, and the shop items bought for it, which the game hasn't received yet. editable says whether save/edits can put them into a save a device sends.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<WaitingResponse> result = await sender.Send(new GetWaitingQuery(User.GetClientId(), User.GetUserId()), ct);

        await SendResultAsync(result, waiting => Results.Ok(waiting));
    }
}
