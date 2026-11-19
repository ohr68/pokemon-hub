namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

// Stored as text, so new values (traded away, received in a trade) can be added without migrating data.
public enum OwnershipChange
{
    Obtained = 1,
    Released = 2,
}
