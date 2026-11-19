namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

// Stored as text, so new values can be added without migrating data.
public enum OwnershipChange
{
    Obtained = 1,
    Released = 2,
    TradedAway = 3,
    ReceivedInTrade = 4,
    EvolvedByTrade = 5,
}
