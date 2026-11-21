using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Shop;

/// <summary>
/// A Pokémon for sale in the hub's shop, set up by an admin. Each purchase gives the buyer a
/// new Pokémon of this species, level and shininess in their hub.
/// </summary>
public sealed class ShopOffer : Entity
{
    private ShopOffer()
    {
    }

    public Guid Id { get; private set; }

    public int SpeciesId { get; private set; }

    // Copied from the Pokédex when the offer is made, to show it without asking.
    public string SpeciesName { get; private set; }

    public bool IsRare { get; private set; }

    public int Level { get; private set; }

    public bool IsShiny { get; private set; }

    public int Price { get; private set; }

    // How many are left; empty means no limit.
    public int? Stock { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public bool IsAvailable => IsActive && Stock is null or > 0;

    public static ShopOffer Create(int speciesId, string speciesName, bool isRare, int level, bool isShiny, int price, int? stock, DateTime utcNow) => new()
    {
        Id = Guid.CreateVersion7(),
        SpeciesId = speciesId,
        SpeciesName = speciesName,
        IsRare = isRare,
        Level = level,
        IsShiny = isShiny,
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

    /// <summary>Puts one back, after a purchase failed.</summary>
    public void Release()
    {
        if (Stock is not null)
        {
            Stock++;
        }
    }
}
