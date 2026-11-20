namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

/// <summary>How a Pokémon's place in a game save changed; games hear about it through webhooks.</summary>
public enum SaveChange
{
    // Moved into the save on PokeHub; waiting for the game to deliver it.
    MovedIn = 1,

    // Moved back to the hub before the game delivered it.
    MovedOut = 2,

    // The game wrote it into its own save.
    Delivered = 3,

    // The game sent it from its own save to the hub.
    Withdrawn = 4,

    // Released from the save.
    Released = 5,

    // Found gone from the game when its save was mirrored.
    LeftGame = 6,
}
