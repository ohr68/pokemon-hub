using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Trades.Domain.Trades;

/// <summary>
/// A one-for-one offer between two trainers, like a link trade: the proposer offers one of
/// their Pokémon for one of the recipient's. The recipient accepts or rejects it, the proposer
/// can cancel it, and it expires if nobody answers in time.
/// </summary>
public sealed class Trade : Entity
{
    public static readonly TimeSpan OfferLifetime = TimeSpan.FromDays(7);

    private Trade()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProposerId { get; private set; }

    public string ProposerName { get; private set; }

    public Guid RecipientId { get; private set; }

    public string RecipientName { get; private set; }

    public TradedPokemon Offered { get; private set; }

    public TradedPokemon Requested { get; private set; }

    public TradeStatus Status { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public DateTime ExpiresOnUtc { get; private set; }

    public DateTime? ResolvedOnUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public static Result<Trade> Propose(
        Guid proposerId,
        string proposerName,
        Guid recipientId,
        string recipientName,
        TradedPokemon offered,
        TradedPokemon requested,
        DateTime utcNow)
    {
        if (proposerId == recipientId)
        {
            return Result.Failure<Trade>(TradeErrors.CannotTradeWithYourself);
        }

        var trade = new Trade
        {
            Id = Guid.CreateVersion7(),
            ProposerId = proposerId,
            ProposerName = proposerName,
            RecipientId = recipientId,
            RecipientName = recipientName,
            Offered = offered,
            Requested = requested,
            Status = TradeStatus.Pending,
            CreatedOnUtc = utcNow,
            ExpiresOnUtc = utcNow.Add(OfferLifetime),
        };

        trade.Raise(new TradeProposedDomainEvent(trade.Id, proposerId, recipientId));

        return trade;
    }

    /// <summary>
    /// Whether the trainer is one of the two sides. Nobody else may see or act on the trade.
    /// </summary>
    public bool Involves(Guid trainerId) => trainerId == ProposerId || trainerId == RecipientId;

    /// <summary>
    /// Checks the recipient can accept now. An offer past its expiry is marked expired.
    /// </summary>
    public Result EnsureCanAccept(Guid trainerId, DateTime utcNow)
    {
        Result answer = EnsurePendingAnswerFrom(trainerId);

        if (answer.IsFailure)
        {
            return answer;
        }

        if (utcNow >= ExpiresOnUtc)
        {
            Resolve(TradeStatus.Expired, utcNow);

            return Result.Failure(TradeErrors.Expired);
        }

        return Result.Success();
    }

    public void Complete(DateTime utcNow)
    {
        Resolve(TradeStatus.Completed, utcNow);

        Raise(new TradeCompletedDomainEvent(Id, ProposerId, RecipientId));
    }

    public void Fail(string reason, DateTime utcNow)
    {
        FailureReason = reason;

        Resolve(TradeStatus.Failed, utcNow);
    }

    public Result Reject(Guid trainerId, DateTime utcNow)
    {
        Result answer = EnsurePendingAnswerFrom(trainerId);

        if (answer.IsFailure)
        {
            return answer;
        }

        Resolve(TradeStatus.Rejected, utcNow);

        return Result.Success();
    }

    public Result Cancel(Guid trainerId, DateTime utcNow)
    {
        if (trainerId != ProposerId)
        {
            return Result.Failure(TradeErrors.OnlyProposerCanCancel);
        }

        if (Status != TradeStatus.Pending)
        {
            return Result.Failure(TradeErrors.NotPending);
        }

        Resolve(TradeStatus.Cancelled, utcNow);

        return Result.Success();
    }

    private Result EnsurePendingAnswerFrom(Guid trainerId)
    {
        if (trainerId != RecipientId)
        {
            return Result.Failure(TradeErrors.OnlyRecipientCanAnswer);
        }

        return Status == TradeStatus.Pending ? Result.Success() : Result.Failure(TradeErrors.NotPending);
    }

    private void Resolve(TradeStatus status, DateTime utcNow)
    {
        Status = status;
        ResolvedOnUtc = utcNow;
    }
}
