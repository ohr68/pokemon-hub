namespace PokeHub.Modules.Trades.Domain.Trades;

public enum TradeStatus
{
    Pending = 1,
    Completed = 2,
    Rejected = 3,
    Cancelled = 4,
    Expired = 5,

    // Accepted, but the swap couldn't happen (a Pokémon was released or traded elsewhere).
    Failed = 6,
}
