using System.Data.Common;
using Dapper;
using FluentValidation;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.Application.Abstractions.Data;
using PokeHub.Modules.Economy.Domain.Shop;
using PokeHub.Modules.Economy.Domain.Wallets;
using PokeHub.Modules.Games.PublicApi;

namespace PokeHub.Modules.Economy.Application.Shop;

/// <param name="Quantity">How many of the item one purchase gives.</param>
/// <param name="Stock">How many purchases can be made; empty for no limit.</param>
public sealed record CreateItemOfferCommand(int ItemId, int Quantity, int Price, int? Stock) : ICommand<Guid>;

public sealed record UpdateItemOfferCommand(Guid OfferId, int Price, int? Stock, bool IsActive) : ICommand;

/// <param name="GameId">The game whose bag gets the item.</param>
public sealed record BuyItemOfferCommand(Guid TrainerId, Guid OfferId, Guid GameId) : ICommand<ItemPurchaseResultResponse>;

public sealed record ItemPurchaseResultResponse(Guid PurchaseId, long Balance);

/// <summary>A game wrote these purchases into its save. Repeating it changes nothing.</summary>
public sealed record MarkItemsDeliveredCommand(Guid TrainerId, IReadOnlyCollection<Guid> PurchaseIds) : ICommand;

/// <summary>Items recorded as delivered whose save never reached the game wait for it again.</summary>
public sealed record ReturnItemsCommand(Guid TrainerId, IReadOnlyCollection<Guid> PurchaseIds) : ICommand;

/// <param name="All">Admins see every offer, inactive and sold out ones too.</param>
public sealed record GetItemOffersQuery(bool All) : IQuery<IReadOnlyCollection<ItemOfferResponse>>;

public sealed record ItemOfferResponse(
    Guid Id,
    int ItemId,
    string ItemName,
    string Pocket,
    int Quantity,
    int Price,
    int? Stock,
    bool IsActive);

/// <param name="GameId">Only the items for this game, plus ones bought before a game was chosen.</param>
public sealed record GetAwaitingItemsQuery(Guid TrainerId, Guid? GameId = null) : IQuery<IReadOnlyCollection<AwaitingItemResponse>>;

public sealed record AwaitingItemResponse(
    Guid Id,
    int ItemId,
    string ItemName,
    string Pocket,
    int Quantity,
    DateTime PurchasedOnUtc,
    Guid? GameId,
    string? GameName);

internal sealed class CreateItemOfferCommandValidator : AbstractValidator<CreateItemOfferCommand>
{
    public CreateItemOfferCommandValidator()
    {
        RuleFor(c => c.Quantity).InclusiveBetween(1, 99);
        RuleFor(c => c.Price).InclusiveBetween(1, 1_000_000);
        RuleFor(c => c.Stock).GreaterThanOrEqualTo(0);
    }
}

internal sealed class UpdateItemOfferCommandValidator : AbstractValidator<UpdateItemOfferCommand>
{
    public UpdateItemOfferCommandValidator()
    {
        RuleFor(c => c.Price).InclusiveBetween(1, 1_000_000);
        RuleFor(c => c.Stock).GreaterThanOrEqualTo(0);
    }
}

internal sealed class MarkItemsDeliveredCommandValidator : AbstractValidator<MarkItemsDeliveredCommand>
{
    public MarkItemsDeliveredCommandValidator()
    {
        RuleFor(c => c.PurchaseIds).NotEmpty();
    }
}

internal sealed class CreateItemOfferCommandHandler(
    IShopRepository shopRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateItemOfferCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateItemOfferCommand request, CancellationToken cancellationToken)
    {
        GameItem? item = GameItems.Find(request.ItemId);

        if (item is null)
        {
            return Result.Failure<Guid>(ShopErrors.ItemNotFound(request.ItemId));
        }

        var offer = ItemOffer.Create(item, request.Quantity, request.Price, request.Stock, dateTimeProvider.UtcNow);

        shopRepository.Insert(offer);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return offer.Id;
    }
}

internal sealed class UpdateItemOfferCommandHandler(IShopRepository shopRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateItemOfferCommand>
{
    public async Task<Result> Handle(UpdateItemOfferCommand request, CancellationToken cancellationToken)
    {
        ItemOffer? offer = await shopRepository.GetItemOfferAsync(request.OfferId, cancellationToken);

        if (offer is null)
        {
            return Result.Failure(ShopErrors.OfferNotFound(request.OfferId));
        }

        offer.Update(request.Price, request.Stock, request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>Coins, stock and the purchase are saved together, so a failure leaves nothing half done.</summary>
internal sealed class BuyItemOfferCommandHandler(
    IShopRepository shopRepository,
    IWalletRepository walletRepository,
    IGameSpeciesLimits games,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<BuyItemOfferCommand, ItemPurchaseResultResponse>
{
    public async Task<Result<ItemPurchaseResultResponse>> Handle(BuyItemOfferCommand request, CancellationToken cancellationToken)
    {
        ItemOffer? offer = await shopRepository.GetItemOfferAsync(request.OfferId, cancellationToken);

        if (offer is null)
        {
            return Result.Failure<ItemPurchaseResultResponse>(ShopErrors.OfferNotFound(request.OfferId));
        }

        IReadOnlyDictionary<Guid, GameIdentity> identities = await games.GetIdentitiesAsync([request.GameId], cancellationToken);

        if (!identities.TryGetValue(request.GameId, out GameIdentity? game))
        {
            return Result.Failure<ItemPurchaseResultResponse>(ShopErrors.GameNotFound(request.GameId));
        }

        Wallet? wallet = await walletRepository.GetAsync(request.TrainerId, cancellationToken);

        if (wallet is null)
        {
            return Result.Failure<ItemPurchaseResultResponse>(WalletErrors.NotEnoughCoins(offer.Price, 0));
        }

        Result reserved = offer.Reserve();

        if (reserved.IsFailure)
        {
            return Result.Failure<ItemPurchaseResultResponse>(reserved.Error);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        var purchase = ItemPurchase.Pay(request.TrainerId, offer, request.GameId, game.Name, utcNow);
        Result paid = wallet.Debit(offer.Price, $"item-purchase:{purchase.Id}", utcNow);

        if (paid.IsFailure)
        {
            return Result.Failure<ItemPurchaseResultResponse>(paid.Error);
        }

        shopRepository.Insert(purchase);
        walletRepository.InsertEntries(wallet.NewEntries);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ItemPurchaseResultResponse(purchase.Id, wallet.Balance);
    }
}

internal sealed class MarkItemsDeliveredCommandHandler(
    IShopRepository shopRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MarkItemsDeliveredCommand>
{
    public async Task<Result> Handle(MarkItemsDeliveredCommand request, CancellationToken cancellationToken)
    {
        Guid[] ids = [.. request.PurchaseIds.Distinct()];
        IReadOnlyList<ItemPurchase> purchases = await shopRepository.GetItemPurchasesAsync(request.TrainerId, ids, cancellationToken);

        if (purchases.Count != ids.Length)
        {
            return Result.Failure(ShopErrors.ItemPurchaseNotFound);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        foreach (ItemPurchase purchase in purchases)
        {
            purchase.Delivered(utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class ReturnItemsCommandHandler(IShopRepository shopRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ReturnItemsCommand>
{
    public async Task<Result> Handle(ReturnItemsCommand request, CancellationToken cancellationToken)
    {
        Guid[] ids = [.. request.PurchaseIds.Distinct()];
        IReadOnlyList<ItemPurchase> purchases = await shopRepository.GetItemPurchasesAsync(request.TrainerId, ids, cancellationToken);

        if (purchases.Count != ids.Length)
        {
            return Result.Failure(ShopErrors.ItemPurchaseNotFound);
        }

        foreach (ItemPurchase purchase in purchases)
        {
            purchase.Returned();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class GetItemOffersQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetItemOffersQuery, IReadOnlyCollection<ItemOfferResponse>>
{
    public async Task<Result<IReadOnlyCollection<ItemOfferResponse>>> Handle(GetItemOffersQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT id AS Id, item_id AS ItemId, item_name AS ItemName, pocket AS Pocket, quantity AS Quantity,
                   price AS Price, stock AS Stock, is_active AS IsActive
            FROM economy.item_offers
            WHERE @All OR (is_active AND (stock IS NULL OR stock > 0))
            ORDER BY price, item_id
            """;

        IEnumerable<ItemOfferResponse> offers = await connection.QueryAsync<ItemOfferResponse>(sql, new { request.All });

        return offers.ToList();
    }
}

internal sealed class GetAwaitingItemsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetAwaitingItemsQuery, IReadOnlyCollection<AwaitingItemResponse>>
{
    public async Task<Result<IReadOnlyCollection<AwaitingItemResponse>>> Handle(GetAwaitingItemsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT id AS Id, item_id AS ItemId, item_name AS ItemName, pocket AS Pocket, quantity AS Quantity,
                   purchased_on_utc AS PurchasedOnUtc, game_id AS GameId, game_name AS GameName
            FROM economy.item_purchases
            WHERE trainer_id = @TrainerId AND delivered_on_utc IS NULL
              AND (@GameId::uuid IS NULL OR game_id IS NULL OR game_id = @GameId)
            ORDER BY purchased_on_utc
            """;

        IEnumerable<AwaitingItemResponse> items = await connection.QueryAsync<AwaitingItemResponse>(sql, new { request.TrainerId, request.GameId });

        return items.ToList();
    }
}
