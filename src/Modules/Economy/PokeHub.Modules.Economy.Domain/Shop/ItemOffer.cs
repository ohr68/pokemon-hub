using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Shop;

/// <summary>A bag item for sale, set up by an admin. Each purchase gives <see cref="Quantity"/> of it.</summary>
public sealed class ItemOffer : Entity
{
    private ItemOffer()
    {
    }

    public Guid Id { get; private set; }

    public int ItemId { get; private set; }

    public string ItemName { get; private set; }

    public string Pocket { get; private set; }

    public int Quantity { get; private set; }

    public int Price { get; private set; }

    // How many purchases are left; empty means no limit.
    public int? Stock { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public bool IsAvailable => IsActive && Stock is null or > 0;

    public static ItemOffer Create(GameItem item, int quantity, int price, int? stock, DateTime utcNow) => new()
    {
        Id = Guid.CreateVersion7(),
        ItemId = item.Id,
        ItemName = item.Name,
        Pocket = item.Pocket,
        Quantity = quantity,
        Price = price,
        Stock = stock,
        IsActive = true,
        CreatedOnUtc = utcNow,
    };

    public void Update(int price, int? stock, bool isActive)
    {
        Price = price;
        Stock = stock;
        IsActive = isActive;
    }

    /// <summary>Takes one for a purchase.</summary>
    public Result Reserve()
    {
        if (!IsAvailable)
        {
            return Result.Failure(ShopErrors.Unavailable);
        }

        if (Stock is not null)
        {
            Stock--;
        }

        return Result.Success();
    }
}
