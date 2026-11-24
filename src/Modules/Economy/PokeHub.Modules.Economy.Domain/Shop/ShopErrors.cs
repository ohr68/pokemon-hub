using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Shop;

public static class ShopErrors
{
    public static Error OfferNotFound(Guid offerId) => Error.NotFound("Shop.OfferNotFound", $"No offer {offerId} was found.");

    public static Error SpeciesNotFound(int speciesId) => Error.NotFound("Shop.SpeciesNotFound", $"There is no species {speciesId} in the Pokédex.");

    public static Error ItemNotFound(int itemId) => Error.NotFound("Shop.ItemNotFound", $"Item {itemId} can't be sold.");

    public static Error GameNotFound(Guid gameId) => Error.NotFound("Shop.GameNotFound", $"There is no game {gameId} to send the item to.");

    public static readonly Error ItemPurchaseNotFound = Error.NotFound("Shop.ItemPurchaseNotFound", "One of these item purchases doesn't exist or isn't yours.");

    public static readonly Error Unavailable = Error.Conflict("Shop.Unavailable", "This offer is sold out or no longer for sale.");
}
