using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Presentation.Play.Docs;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed class GetDeliveriesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("save/deliveries");
        Policies(GamesPermissions.ReadPokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<DeliveryInfo>>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetDeliveriesEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<DeliveryInfo>> result = await sender.Send(
            new GetDeliveriesQuery(User.GetClientId(), User.GetUserId()),
            ct);

        await SendResultAsync(result, deliveries => Results.Ok(deliveries));
    }
}
