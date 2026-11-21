using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Economy.Application.Shop;
using PokeHub.Modules.Economy.Application.Wallets;

namespace PokeHub.Modules.Economy.Presentation.Coins;

internal sealed class EconomyEndpointGroup : Group
{
    public EconomyEndpointGroup()
    {
        Configure("economy", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Economy")
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesProblem(StatusCodes.Status409Conflict)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}

internal sealed class GetMyWalletEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("me/wallet");
        Policies(EconomyPermissions.ReadEconomy);
        Group<EconomyEndpointGroup>();

        Description(builder => builder.Produces<WalletResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Gets the signed-in trainer's coins";
            s.Description = "The balance and the latest coins earned and spent. Coins come from shiny and rare (legendary or mythical) catches, badges, trophies and new levels.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<WalletResponse> result = await sender.Send(new GetWalletQuery(User.GetUserId()), ct);

        await SendResultAsync(result, wallet => Results.Ok(wallet));
    }
}

internal sealed class GetMyPurchasesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("me/purchases");
        Policies(EconomyPermissions.ReadEconomy);
        Group<EconomyEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<PurchaseResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "Lists the signed-in trainer's latest 20 purchases");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<PurchaseResponse>> result = await sender.Send(new GetMyPurchasesQuery(User.GetUserId()), ct);

        await SendResultAsync(result, purchases => Results.Ok(purchases));
    }
}
