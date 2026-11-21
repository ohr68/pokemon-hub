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

    public Task<ItemOffer?> GetItemOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        return context.ItemOffers.SingleOrDefaultAsync(o => o.Id == offerId, cancellationToken);
    }

    public async Task<IReadOnlyList<ItemPurchase>> GetItemPurchasesAsync(
        Guid trainerId,
        IReadOnlyCollection<Guid> purchaseIds,
        CancellationToken cancellationToken = default)
    {
        return await context.ItemPurchases
            .Where(p => p.TrainerId == trainerId && purchaseIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public void Insert(ItemOffer offer)
    {
        context.ItemOffers.Add(offer);
    }

    public void Insert(ItemPurchase purchase)
    {
        context.ItemPurchases.Add(purchase);
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
