using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Wallets;

public static class WalletErrors
{
    public static Error NotEnoughCoins(int price, long balance) => Error.Problem(
        "Economy.NotEnoughCoins",
        $"This costs {price} coins and you have {balance}.");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "Economy.ChangedMeanwhile",
        "Your coins changed at the same time. Try again.");
}
