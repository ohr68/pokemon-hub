using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Shop;

public enum PurchaseStatus
{
    // Coins taken; the Pokémon is being given.
    Paid = 1,
    Delivered = 2,
    // The Pokémon couldn't be given; coins and stock went back.
    Refunded = 3,
}

/// <summary>A trainer bought a shop offer. What they bought is copied from the offer.</summary>
public sealed class Purchase : Entity
{
    private Purchase()
    {
    }

    public Guid Id { get; private set; }

    public Guid TrainerId { get; private set; }

    public Guid OfferId { get; private set; }

    public int SpeciesId { get; private set; }

    public string SpeciesName { get; private set; }

    public int Level { get; private set; }

    public bool IsShiny { get; private set; }

    public int Price { get; private set; }

    public PurchaseStatus Status { get; private set; }

    // The Pokémon given (the same id as the purchase).
    public Guid? PokemonId { get; private set; }

    public DateTime PurchasedOnUtc { get; private set; }

    public static Purchase Pay(Guid trainerId, ShopOffer offer, DateTime utcNow) => new()
    {
        Id = Guid.CreateVersion7(),
        TrainerId = trainerId,
        OfferId = offer.Id,
        SpeciesId = offer.SpeciesId,
        SpeciesName = offer.SpeciesName,
        Level = offer.Level,
        IsShiny = offer.IsShiny,
        Price = offer.Price,
        Status = PurchaseStatus.Paid,
        PurchasedOnUtc = utcNow,
    };

    public void Delivered(Guid pokemonId)
    {
        Status = PurchaseStatus.Delivered;
        PokemonId = pokemonId;
    }

    public void Refunded()
    {
        Status = PurchaseStatus.Refunded;
    }
}
