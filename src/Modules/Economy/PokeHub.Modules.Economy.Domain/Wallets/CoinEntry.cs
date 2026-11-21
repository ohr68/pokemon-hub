namespace PokeHub.Modules.Economy.Domain.Wallets;

/// <summary>
/// One change to a wallet: coins earned (positive) or spent (negative). <see cref="Source"/>
/// names what caused it (shiny:{pokemon}, trophy:{code}, purchase:{id}, ...), so nothing pays twice.
/// </summary>
public sealed class CoinEntry
{
    private CoinEntry()
    {
    }

    public Guid Id { get; private set; }

    public Guid TrainerId { get; private set; }

    public CoinReason Reason { get; private set; }

    public int Amount { get; private set; }

    public string Source { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    internal static CoinEntry Create(Guid trainerId, CoinReason reason, int amount, string source, DateTime occurredOnUtc) => new()
    {
        Id = Guid.CreateVersion7(),
        TrainerId = trainerId,
        Reason = reason,
        Amount = amount,
        Source = source,
        OccurredOnUtc = occurredOnUtc,
    };
}
