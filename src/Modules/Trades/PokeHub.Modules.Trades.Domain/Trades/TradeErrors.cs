using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Trades.Domain.Trades;

public static class TradeErrors
{
    public static Error NotFound(Guid tradeId)
    {
        return Error.NotFound("Trades.NotFound", $"The trade with the identifier {tradeId} was not found.");
    }

    public static Error PokemonNotAvailable(Guid pokemonId)
    {
        return Error.Problem(
            "Trades.PokemonNotAvailable",
            $"The Pokémon {pokemonId} is not with that trainer anymore.");
    }

    public static Error TrainerNotFound(Guid trainerId)
    {
        return Error.NotFound("Trades.TrainerNotFound", $"The trainer with the identifier {trainerId} was not found.");
    }

    public static readonly Error CannotTradeWithYourself = Error.Problem(
        "Trades.CannotTradeWithYourself",
        "A trade needs two different trainers.");

    public static readonly Error NotPending = Error.Conflict(
        "Trades.NotPending",
        "The trade is not waiting for an answer anymore.");

    public static readonly Error Expired = Error.Conflict(
        "Trades.Expired",
        "The trade expired before it was accepted.");

    public static readonly Error OnlyRecipientCanAnswer = Error.Problem(
        "Trades.OnlyRecipientCanAnswer",
        "Only the trainer who received the offer can accept or reject it.");

    public static readonly Error OnlyProposerCanCancel = Error.Problem(
        "Trades.OnlyProposerCanCancel",
        "Only the trainer who made the offer can cancel it.");
}
