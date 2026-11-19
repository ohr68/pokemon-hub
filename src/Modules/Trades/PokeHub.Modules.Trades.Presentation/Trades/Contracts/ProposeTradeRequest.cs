namespace PokeHub.Modules.Trades.Presentation.Trades.Contracts;

internal sealed record ProposeTradeRequest(Guid RecipientId, Guid OfferedPokemonId, Guid RequestedPokemonId);
