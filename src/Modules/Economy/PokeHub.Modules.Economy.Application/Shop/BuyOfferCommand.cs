using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.Application.Abstractions.Data;
using PokeHub.Modules.Economy.Domain.Shop;
using PokeHub.Modules.Economy.Domain.Wallets;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Economy.Application.Shop;

public sealed record BuyOfferCommand(Guid TrainerId, Guid OfferId) : ICommand<PurchaseResultResponse>;

/// <param name="PokemonId">The Pokémon now in the buyer's hub.</param>
public sealed record PurchaseResultResponse(Guid PurchaseId, Guid PokemonId, long Balance);

/// <summary>
/// Pays first (coins and stock, in one save), then asks the Pokédex for the Pokémon. If that
/// fails, the coins and the stock go back and the purchase is marked refunded.
/// </summary>
internal sealed class BuyOfferCommandHandler(
    IShopRepository shopRepository,
    IWalletRepository walletRepository,
    IPokedexApi pokedexApi,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<BuyOfferCommand, PurchaseResultResponse>
{
    public async Task<Result<PurchaseResultResponse>> Handle(BuyOfferCommand request, CancellationToken cancellationToken)
    {
        ShopOffer? offer = await shopRepository.GetOfferAsync(request.OfferId, cancellationToken);

        if (offer is null)
        {
            return Result.Failure<PurchaseResultResponse>(ShopErrors.OfferNotFound(request.OfferId));
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        Wallet? wallet = await walletRepository.GetAsync(request.TrainerId, cancellationToken);

        if (wallet is null)
        {
            return Result.Failure<PurchaseResultResponse>(WalletErrors.NotEnoughCoins(offer.Price, 0));
        }

        Result reserved = offer.Reserve();

        if (reserved.IsFailure)
        {
            return Result.Failure<PurchaseResultResponse>(reserved.Error);
        }

        var purchase = Purchase.Pay(request.TrainerId, offer, utcNow);
        Result paid = wallet.Debit(offer.Price, $"purchase:{purchase.Id}", utcNow);

        if (paid.IsFailure)
        {
            return Result.Failure<PurchaseResultResponse>(paid.Error);
        }

        shopRepository.Insert(purchase);
        walletRepository.InsertEntries(wallet.NewEntries);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Result<Guid> given = await pokedexApi.GiveBoughtPokemonAsync(
            purchase.Id,
            request.TrainerId,
            purchase.SpeciesId,
            purchase.Level,
            purchase.IsShiny,
            cancellationToken);

        if (given.IsFailure)
        {
            int before = wallet.NewEntries.Count;
            wallet.Credit(CoinReason.Refund, purchase.Price, $"refund:{purchase.Id}", utcNow, utcNow);
            walletRepository.InsertEntries(wallet.NewEntries.Skip(before));
            offer.Release();
            purchase.Refunded();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<PurchaseResultResponse>(given.Error);
        }

        purchase.Delivered(given.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PurchaseResultResponse(purchase.Id, given.Value, wallet.Balance);
    }
}
