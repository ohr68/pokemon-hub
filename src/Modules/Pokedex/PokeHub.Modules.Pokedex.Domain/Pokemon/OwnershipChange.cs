namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

// Stored as text, so new values can be added without migrating data.
public enum OwnershipChange
{
    Obtained = 1,
    Released = 2,
    TradedAway = 3,
    ReceivedInTrade = 4,
    EvolvedByTrade = 5,

    // Location changes: the trainer keeps the Pokémon, it just moves between hub and a save.
    MovedToSave = 6,
    MovedToHub = 7,

    // Gone from the game it was caught in (released or traded away inside the game), found
    // when PokeHub mirrors the game's save.
    LeftGame = 8,
}
