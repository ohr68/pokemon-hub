using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Shop;

/// <summary>
/// Bag items a trainer bought. They wait here until a game writes them into its save. What was
/// bought is copied from the offer.
/// </summary>
public sealed class ItemPurchase : Entity
{
    private ItemPurchase()
    {
    }

    public Guid Id { get; private set; }

    public Guid TrainerId { get; private set; }

    public Guid OfferId { get; private set; }

    public int ItemId { get; private set; }

    public string ItemName { get; private set; }

    public string Pocket { get; private set; }

    public int Quantity { get; private set; }

    public int Price { get; private set; }

    public DateTime PurchasedOnUtc { get; private set; }

    public DateTime? DeliveredOnUtc { get; private set; }

    public static ItemPurchase Pay(Guid trainerId, ItemOffer offer, DateTime utcNow) => new()
    {
        Id = Guid.CreateVersion7(),
        TrainerId = trainerId,
        OfferId = offer.Id,
        ItemId = offer.ItemId,
        ItemName = offer.ItemName,
        Pocket = offer.Pocket,
        Quantity = offer.Quantity,
        Price = offer.Price,
        PurchasedOnUtc = utcNow,
    };

    public void Delivered(DateTime utcNow)
    {
        DeliveredOnUtc ??= utcNow;
    }
}
