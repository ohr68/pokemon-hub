namespace PokeHub.Modules.Economy.Domain.Shop;

public interface IShopRepository
{
    Task<ShopOffer?> GetOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    void Insert(ShopOffer offer);

    void Insert(Purchase purchase);
}
