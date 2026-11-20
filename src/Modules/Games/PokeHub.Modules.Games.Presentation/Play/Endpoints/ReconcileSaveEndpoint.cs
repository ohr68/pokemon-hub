using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Presentation.Play.Contracts;
using PokeHub.Modules.Games.Presentation.Play.Docs;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed class ReconcileSaveEndpoint(ISender sender) : EndpointBase<ReconcileSaveRequest>
{
    public override void Configure()
    {
        Post("save/reconcile");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<LeftGameInfo>>(StatusCodes.Status200OK, "application/json"));
        Summary(new ReconcileSaveEndpointSummary());
    }

    public override async Task HandleAsync(ReconcileSaveRequest req, CancellationToken ct)
    {
        Result<IReadOnlyCollection<LeftGameInfo>> result = await sender.Send(
            new ReconcileSaveInGameCommand(User.GetClientId(), User.GetUserId(), req.PresentCatchKeys, req.DryRun),
            ct);

        await SendResultAsync(result, left => Results.Ok(left));
    }
}
