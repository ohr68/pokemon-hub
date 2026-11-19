using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.PublicApi;
using PokeHub.Modules.Trades.Application.Abstractions.Data;
using PokeHub.Modules.Trades.Domain.Trades;

namespace PokeHub.Modules.Trades.Application.Trades.AcceptTrade;

// The simple version: the swap happens right away, in process, through the Pokédex. The
// Pokédex commits first; if saving the trade then fails, accepting again completes it without
// swapping twice, because the Pokédex recognises the trade id.
internal sealed class AcceptTradeCommandHandler(
    ITradeRepository tradeRepository,
    IPokedexApi pokedexApi,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AcceptTradeCommand>
{
    public async Task<Result> Handle(AcceptTradeCommand request, CancellationToken cancellationToken)
    {
        Trade? trade = await tradeRepository.GetAsync(request.TradeId, cancellationToken);

        // Only the two trainers in a trade can see it.
        if (trade is null || !trade.Involves(request.TrainerId))
        {
            return Result.Failure(TradeErrors.NotFound(request.TradeId));
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        Result canAccept = trade.EnsureCanAccept(request.TrainerId, utcNow);

        if (canAccept.IsFailure)
        {
            // Saves the expiry when the offer turned out to be expired.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return canAccept;
        }

        Result swap = await pokedexApi.TradePokemonAsync(
            new TradeRequest(
                trade.Id,
                trade.ProposerId,
                trade.Offered.PokemonId,
                trade.RecipientId,
                trade.Requested.PokemonId),
            cancellationToken);

        if (swap.IsFailure)
        {
            trade.Fail(swap.Error.Description, utcNow);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return swap;
        }

        trade.Complete(utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
