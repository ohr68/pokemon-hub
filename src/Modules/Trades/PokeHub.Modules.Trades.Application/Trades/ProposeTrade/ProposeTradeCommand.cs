using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Trades.Application.Trades.ProposeTrade;

/// <returns>The identifier of the new trade.</returns>
public sealed record ProposeTradeCommand(
    Guid ProposerId,
    Guid RecipientId,
    Guid OfferedPokemonId,
    Guid RequestedPokemonId) : ICommand<Guid>;
