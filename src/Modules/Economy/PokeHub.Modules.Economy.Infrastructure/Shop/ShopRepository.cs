using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Economy.Domain.Shop;
using PokeHub.Modules.Economy.Infrastructure.Database;

namespace PokeHub.Modules.Economy.Infrastructure.Shop;

internal sealed class ShopRepository(EconomyDbContext context) : IShopRepository
{
    public Task<ShopOffer?> GetOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        return context.ShopOffers.SingleOrDefaultAsync(o => o.Id == offerId, cancellationToken);
    }

    public void Insert(ShopOffer offer)
    {
        context.ShopOffers.Add(offer);
    }

    public void Insert(Purchase purchase)
    {
        context.Purchases.Add(purchase);
    }
}
