namespace PokeHub.Modules.Trades.Application.Trades.GetMyTrades;

/// <param name="Direction">Outgoing when the trainer proposed it, Incoming when they received it.</param>
public sealed record TradeResponse(
    Guid Id,
    string Direction,
    string Status,
    Guid OtherTrainerId,
    string OtherTrainerName,
    TradedPokemonResponse Offered,
    TradedPokemonResponse Requested,
    DateTime CreatedOnUtc,
    DateTime ExpiresOnUtc,
    DateTime? ResolvedOnUtc,
    string? FailureReason);

public sealed record TradedPokemonResponse(
    Guid PokemonId,
    int SpeciesId,
    string SpeciesName,
    string? Nickname,
    int Level,
    bool IsShiny);
