namespace PokeHub.Modules.Economy.Domain.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetAsync(Guid trainerId, CancellationToken cancellationToken = default);

    // The sources among these the trainer already has coins for.
    Task<IReadOnlySet<string>> GetPaidSourcesAsync(Guid trainerId, IReadOnlyCollection<string> sources, CancellationToken cancellationToken = default);

    void Insert(Wallet wallet);

    void InsertEntries(IEnumerable<CoinEntry> entries);
}
