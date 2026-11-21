namespace PokeHub.Modules.Economy.Domain.Shop;

public interface IShopRepository
{
    Task<ShopOffer?> GetOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task<ItemOffer?> GetItemOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemPurchase>> GetItemPurchasesAsync(
        Guid trainerId,
        IReadOnlyCollection<Guid> purchaseIds,
        CancellationToken cancellationToken = default);

    void Insert(ItemOffer offer);

    void Insert(ItemPurchase purchase);

    void Insert(ShopOffer offer);

    void Insert(Purchase purchase);
}
