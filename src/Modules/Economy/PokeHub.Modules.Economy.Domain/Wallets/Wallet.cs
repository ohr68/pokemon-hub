using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Domain.Wallets;

/// <summary>
/// A trainer's coins. The trainer id is the same as in the Pokédex (and the user's id).
/// Created with their first coins; a trainer without one has none.
/// </summary>
public sealed class Wallet : Entity
{
    private readonly List<CoinEntry> _newEntries = [];

    private Wallet()
    {
    }

    public Guid TrainerId { get; private set; }

    public long Balance { get; private set; }

    public DateTime UpdatedOnUtc { get; private set; }

    // Changes since loading, for the repository to save.
    public IReadOnlyCollection<CoinEntry> NewEntries => _newEntries.AsReadOnly();

    public static Wallet Open(Guid trainerId, DateTime utcNow) => new()
    {
        TrainerId = trainerId,
        UpdatedOnUtc = utcNow,
    };

    public void Credit(CoinReason reason, int amount, string source, DateTime occurredOnUtc, DateTime utcNow)
    {
        if (amount <= 0)
        {
            return;
        }

        _newEntries.Add(CoinEntry.Create(TrainerId, reason, amount, source, occurredOnUtc));
        Balance += amount;
        UpdatedOnUtc = utcNow;
    }

    public Result Debit(int amount, string source, DateTime utcNow)
    {
        if (amount > Balance)
        {
            return Result.Failure(WalletErrors.NotEnoughCoins(amount, Balance));
        }

        _newEntries.Add(CoinEntry.Create(TrainerId, CoinReason.Purchase, -amount, source, utcNow));
        Balance -= amount;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }
}
