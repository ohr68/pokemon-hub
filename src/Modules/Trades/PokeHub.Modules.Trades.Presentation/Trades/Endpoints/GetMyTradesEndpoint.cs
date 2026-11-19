using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Trades.Application.Trades.GetMyTrades;
using PokeHub.Modules.Trades.Presentation.Trades.Docs;

namespace PokeHub.Modules.Trades.Presentation.Trades.Endpoints;

internal sealed class GetMyTradesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("/");
        Policies(TradesPermissions.ReadTrades);
        Group<TradesEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<TradeResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetMyTradesEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<TradeResponse>> result = await sender.Send(new GetMyTradesQuery(User.GetUserId()), ct);

        await SendResultAsync(result, trades => Results.Ok(trades));
    }
}
