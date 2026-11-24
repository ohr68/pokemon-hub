using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Shop;

/// <summary>
/// Bag items a trainer bought for one of their games. They wait here until that game writes them
/// into its save. What was bought is copied from the offer, and the game's name from the game.
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

    // Empty only for purchases made before the player chose a game; any game may take those.
    public Guid? GameId { get; private set; }

    public string? GameName { get; private set; }

    public DateTime PurchasedOnUtc { get; private set; }

    public DateTime? DeliveredOnUtc { get; private set; }

    public static ItemPurchase Pay(Guid trainerId, ItemOffer offer, Guid gameId, string gameName, DateTime utcNow) => new()
    {
        Id = Guid.CreateVersion7(),
        TrainerId = trainerId,
        OfferId = offer.Id,
        ItemId = offer.ItemId,
        ItemName = offer.ItemName,
        Pocket = offer.Pocket,
        Quantity = offer.Quantity,
        Price = offer.Price,
        GameId = gameId,
        GameName = gameName,
        PurchasedOnUtc = utcNow,
    };

    public void Delivered(DateTime utcNow)
    {
        DeliveredOnUtc ??= utcNow;
    }
}
