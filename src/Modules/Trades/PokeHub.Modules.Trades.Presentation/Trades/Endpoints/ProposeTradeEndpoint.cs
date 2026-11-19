using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Trades.Application.Trades.ProposeTrade;
using PokeHub.Modules.Trades.Presentation.Trades.Contracts;
using PokeHub.Modules.Trades.Presentation.Trades.Docs;

namespace PokeHub.Modules.Trades.Presentation.Trades.Endpoints;

internal sealed class ProposeTradeEndpoint(ISender sender) : EndpointBase<ProposeTradeRequest>
{
    public override void Configure()
    {
        Post("/");
        Policies(TradesPermissions.UpdateTrades);
        Group<TradesEndpointGroup>();

        Description(builder => builder.Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(new ProposeTradeEndpointSummary());
    }

    public override async Task HandleAsync(ProposeTradeRequest req, CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(
            new ProposeTradeCommand(User.GetUserId(), req.RecipientId, req.OfferedPokemonId, req.RequestedPokemonId),
            ct);

        await SendResultAsync(result, tradeId => Results.Ok(tradeId));
    }
}
