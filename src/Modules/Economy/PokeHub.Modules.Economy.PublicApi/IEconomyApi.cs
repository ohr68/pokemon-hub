using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.PublicApi;

public interface IEconomyApi
{
    /// <summary>Bag items the trainer bought for this game (or before choosing one) that it hasn't received yet.</summary>
    Task<IReadOnlyCollection<AwaitingItem>> GetAwaitingItemsAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default);

    /// <summary>These items were written into the game's save. All are recorded, or none.</summary>
    Task<Result> MarkItemsDeliveredAsync(Guid trainerId, IReadOnlyCollection<Guid> purchaseIds, CancellationToken cancellationToken = default);

    /// <summary>The save with these items never reached the game: they wait for it again.</summary>
    Task<Result> ReturnItemsAsync(Guid trainerId, IReadOnlyCollection<Guid> purchaseIds, CancellationToken cancellationToken = default);
}

/// <param name="Pocket">The bag pocket it goes into: items or balls.</param>
/// <param name="ItemId">Its number in Gen 3 games.</param>
public sealed record AwaitingItem(Guid PurchaseId, int ItemId, string ItemName, string Pocket, int Quantity);
