using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Economy.Domain.Wallets;
using PokeHub.Modules.Economy.Infrastructure.Database;

namespace PokeHub.Modules.Economy.Infrastructure.Wallets;

internal sealed class WalletRepository(EconomyDbContext context) : IWalletRepository
{
    public Task<Wallet?> GetAsync(Guid trainerId, CancellationToken cancellationToken = default)
    {
        return context.Wallets.SingleOrDefaultAsync(w => w.TrainerId == trainerId, cancellationToken);
    }

    public async Task<IReadOnlySet<string>> GetPaidSourcesAsync(Guid trainerId, IReadOnlyCollection<string> sources, CancellationToken cancellationToken = default)
    {
        List<string> paid = await context.CoinEntries
            .Where(e => e.TrainerId == trainerId && sources.Contains(e.Source))
            .Select(e => e.Source)
            .ToListAsync(cancellationToken);

        return paid.ToHashSet(StringComparer.Ordinal);
    }

    public void Insert(Wallet wallet)
    {
        context.Wallets.Add(wallet);
    }

    public void InsertEntries(IEnumerable<CoinEntry> entries)
    {
        context.CoinEntries.AddRange(entries);
    }
}
