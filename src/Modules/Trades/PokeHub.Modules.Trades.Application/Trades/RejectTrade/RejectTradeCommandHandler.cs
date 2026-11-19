using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Trades.Application.Abstractions.Data;
using PokeHub.Modules.Trades.Domain.Trades;

namespace PokeHub.Modules.Trades.Application.Trades.RejectTrade;

internal sealed class RejectTradeCommandHandler(
    ITradeRepository tradeRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RejectTradeCommand>
{
    public async Task<Result> Handle(RejectTradeCommand request, CancellationToken cancellationToken)
    {
        Trade? trade = await tradeRepository.GetAsync(request.TradeId, cancellationToken);

        if (trade is null || !trade.Involves(request.TrainerId))
        {
            return Result.Failure(TradeErrors.NotFound(request.TradeId));
        }

        Result result = trade.Reject(request.TrainerId, dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
