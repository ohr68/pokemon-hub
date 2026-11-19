using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Trades.Application.Trades.CancelTrade;
using PokeHub.Modules.Trades.Presentation.Trades.Contracts;
using PokeHub.Modules.Trades.Presentation.Trades.Docs;

namespace PokeHub.Modules.Trades.Presentation.Trades.Endpoints;

internal sealed class CancelTradeEndpoint(ISender sender) : EndpointBase<TradeRouteRequest>
{
    public override void Configure()
    {
        Post("{tradeId:guid}/cancel");
        Policies(TradesPermissions.UpdateTrades);
        Group<TradesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new CancelTradeEndpointSummary());
    }

    public override async Task HandleAsync(TradeRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new CancelTradeCommand(req.TradeId, User.GetUserId()), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
