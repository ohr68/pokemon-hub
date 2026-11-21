using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Economy.Application.Shop;
using PokeHub.Modules.Economy.Presentation.Coins;

namespace PokeHub.Modules.Economy.Presentation.Shop;

internal sealed class OfferRouteRequest
{
    public Guid OfferId { get; init; }
}

internal sealed record CreateOfferRequest(int SpeciesId, int Level, bool IsShiny, int Price, int? Stock);

internal sealed class UpdateOfferRequest
{
    public Guid OfferId { get; init; }

    public int Price { get; init; }

    public int? Stock { get; init; }

    public bool IsActive { get; init; }
}

internal sealed class GetShopEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("shop");
        Policies(EconomyPermissions.ReadEconomy);
        Group<EconomyEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<OfferResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "Lists the Pokémon for sale");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<OfferResponse>> result = await sender.Send(new GetOffersQuery(All: false), ct);

        await SendResultAsync(result, offers => Results.Ok(offers));
    }
}

internal sealed class BuyOfferEndpoint(ISender sender) : EndpointBase<OfferRouteRequest>
{
    public override void Configure()
    {
        Post("shop/{offerId:guid}/buy");
        Policies(EconomyPermissions.Buy);
        Group<EconomyEndpointGroup>();

        Description(builder => builder.Produces<PurchaseResultResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Buys a shop offer with coins";
            s.Description = "The Pokémon goes to the buyer's hub, from where it can be moved into a game. If it can't be given, the coins are refunded.";
        });
    }

    public override async Task HandleAsync(OfferRouteRequest req, CancellationToken ct)
    {
        Result<PurchaseResultResponse> result = await sender.Send(new BuyOfferCommand(User.GetUserId(), req.OfferId), ct);

        await SendResultAsync(result, purchase => Results.Ok(purchase));
    }
}

internal sealed class GetManagedOffersEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("shop/manage");
        Policies(EconomyPermissions.ManageShop);
        Group<EconomyEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<OfferResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "Lists every offer, for admins");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<OfferResponse>> result = await sender.Send(new GetOffersQuery(All: true), ct);

        await SendResultAsync(result, offers => Results.Ok(offers));
    }
}

internal sealed class CreateOfferEndpoint(ISender sender) : EndpointBase<CreateOfferRequest>
{
    public override void Configure()
    {
        Post("shop/offers");
        Policies(EconomyPermissions.ManageShop);
        Group<EconomyEndpointGroup>();

        Description(builder => builder.Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Puts a Pokémon up for sale";
            s.Description = "Species by national number, level, shininess, price in coins and an optional stock.";
        });
    }

    public override async Task HandleAsync(CreateOfferRequest req, CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(new CreateOfferCommand(req.SpeciesId, req.Level, req.IsShiny, req.Price, req.Stock), ct);

        await SendResultAsync(result, id => Results.Ok(id));
    }
}

internal sealed class UpdateOfferEndpoint(ISender sender) : EndpointBase<UpdateOfferRequest>
{
    public override void Configure()
    {
        Put("shop/offers/{offerId:guid}");
        Policies(EconomyPermissions.ManageShop);
        Group<EconomyEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s => s.Summary = "Changes an offer's price, stock or whether it's for sale");
    }

    public override async Task HandleAsync(UpdateOfferRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new UpdateOfferCommand(req.OfferId, req.Price, req.Stock, req.IsActive), ct);

        await SendResultAsync(result, () => Results.NoContent());
    }
}
