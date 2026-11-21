using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Economy.Application.Shop;
using PokeHub.Modules.Economy.Domain.Shop;
using PokeHub.Modules.Economy.Presentation.Coins;

namespace PokeHub.Modules.Economy.Presentation.Shop;

internal sealed record CreateItemOfferRequest(int ItemId, int Quantity, int Price, int? Stock);

internal sealed class UpdateItemOfferRequest
{
    public Guid OfferId { get; init; }

    public int Price { get; init; }

    public int? Stock { get; init; }

    public bool IsActive { get; init; }
}

internal sealed record MarkItemsDeliveredRequest(IReadOnlyCollection<Guid> PurchaseIds);

internal sealed class GetItemOffersEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("shop/items");
        Policies(EconomyPermissions.ReadEconomy);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "Lists the bag items for sale");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<ItemOfferResponse>> result = await sender.Send(new GetItemOffersQuery(All: false), ct);

        await SendResultAsync(result, offers => Results.Ok(offers));
    }
}

internal sealed class BuyItemOfferEndpoint(ISender sender) : EndpointBase<OfferRouteRequest>
{
    public override void Configure()
    {
        Post("shop/items/{offerId:guid}/buy");
        Policies(EconomyPermissions.Buy);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "Buys a bag item with coins; it waits until a game delivers it");
    }

    public override async Task HandleAsync(OfferRouteRequest req, CancellationToken ct)
    {
        Result<ItemPurchaseResultResponse> result = await sender.Send(new BuyItemOfferCommand(User.GetUserId(), req.OfferId), ct);

        await SendResultAsync(result, purchase => Results.Ok(purchase));
    }
}

internal sealed class GetAwaitingItemsEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("me/items");
        Policies(EconomyPermissions.ReadEconomy);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "Bought items waiting to enter a game");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<AwaitingItemResponse>> result = await sender.Send(new GetAwaitingItemsQuery(User.GetUserId()), ct);

        await SendResultAsync(result, items => Results.Ok(items));
    }
}

internal sealed class MarkItemsDeliveredEndpoint(ISender sender) : EndpointBase<MarkItemsDeliveredRequest>
{
    public override void Configure()
    {
        Post("me/items/delivered");
        Policies(EconomyPermissions.Buy);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "Records that these items were written into a game's save");
    }

    public override async Task HandleAsync(MarkItemsDeliveredRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new MarkItemsDeliveredCommand(User.GetUserId(), req.PurchaseIds), ct);

        await SendResultAsync(result, () => Results.NoContent());
    }
}

internal sealed class GetItemCatalogEndpoint : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("shop/items/catalog");
        Policies(EconomyPermissions.ManageShop);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "The items an admin can put up for sale");
    }

    public override Task HandleAsync(CancellationToken ct) => Send.OkAsync(GameItems.All, ct);
}

internal sealed class GetManagedItemOffersEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("shop/items/manage");
        Policies(EconomyPermissions.ManageShop);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "Lists every item offer, for admins");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<ItemOfferResponse>> result = await sender.Send(new GetItemOffersQuery(All: true), ct);

        await SendResultAsync(result, offers => Results.Ok(offers));
    }
}

internal sealed class CreateItemOfferEndpoint(ISender sender) : EndpointBase<CreateItemOfferRequest>
{
    public override void Configure()
    {
        Post("shop/items");
        Policies(EconomyPermissions.ManageShop);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "Puts a bag item up for sale");
    }

    public override async Task HandleAsync(CreateItemOfferRequest req, CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(new CreateItemOfferCommand(req.ItemId, req.Quantity, req.Price, req.Stock), ct);

        await SendResultAsync(result, id => Results.Ok(id));
    }
}

internal sealed class UpdateItemOfferEndpoint(ISender sender) : EndpointBase<UpdateItemOfferRequest>
{
    public override void Configure()
    {
        Put("shop/items/{offerId:guid}");
        Policies(EconomyPermissions.ManageShop);
        Group<EconomyEndpointGroup>();
        Summary(s => s.Summary = "Changes an item offer's price, stock or whether it's for sale");
    }

    public override async Task HandleAsync(UpdateItemOfferRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new UpdateItemOfferCommand(req.OfferId, req.Price, req.Stock, req.IsActive), ct);

        await SendResultAsync(result, () => Results.NoContent());
    }
}
