using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.Application.Abstractions.Data;
using PokeHub.Modules.Economy.Domain.Wallets;

namespace PokeHub.Modules.Economy.Application.Wallets;

/// <param name="Source">What earned them (shiny:{pokemon}, trophy:{code}, ...); each pays once.</param>
public sealed record CoinAward(CoinReason Reason, int Amount, string Source, DateTime OccurredOnUtc);

public sealed record EarnCoinsCommand(Guid TrainerId, IReadOnlyCollection<CoinAward> Awards) : ICommand;

internal sealed class EarnCoinsCommandHandler(
    IWalletRepository repository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<EarnCoinsCommand>
{
    public async Task<Result> Handle(EarnCoinsCommand request, CancellationToken cancellationToken)
    {
        IReadOnlySet<string> paid = await repository.GetPaidSourcesAsync(
            request.TrainerId,
            request.Awards.Select(a => a.Source).ToList(),
            cancellationToken);

        var awards = request.Awards.Where(a => a.Amount > 0 && !paid.Contains(a.Source)).ToList();

        if (awards.Count == 0)
        {
            return Result.Success();
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        Wallet? wallet = await repository.GetAsync(request.TrainerId, cancellationToken);

        if (wallet is null)
        {
            wallet = Wallet.Open(request.TrainerId, utcNow);
            repository.Insert(wallet);
        }

        foreach (CoinAward award in awards)
        {
            wallet.Credit(award.Reason, award.Amount, award.Source, award.OccurredOnUtc, utcNow);
        }

        repository.InsertEntries(wallet.NewEntries);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
