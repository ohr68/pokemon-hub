using MediatR;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.Application.Shop;
using PokeHub.Modules.Economy.PublicApi;

namespace PokeHub.Modules.Economy.Infrastructure.PublicApi;

internal sealed class EconomyApi(ISender sender) : IEconomyApi
{
    public async Task<IReadOnlyCollection<AwaitingItem>> GetAwaitingItemsAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<AwaitingItemResponse>> result = await sender.Send(new GetAwaitingItemsQuery(trainerId, gameId), cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(GetAwaitingItemsQuery), result.Error);
        }

        return [.. result.Value.Select(i => new AwaitingItem(i.Id, i.ItemId, i.ItemName, i.Pocket, i.Quantity))];
    }

    public Task<Result> MarkItemsDeliveredAsync(Guid trainerId, IReadOnlyCollection<Guid> purchaseIds, CancellationToken cancellationToken = default) =>
        sender.Send(new MarkItemsDeliveredCommand(trainerId, purchaseIds), cancellationToken);

    public Task<Result> ReturnItemsAsync(Guid trainerId, IReadOnlyCollection<Guid> purchaseIds, CancellationToken cancellationToken = default) =>
        sender.Send(new ReturnItemsCommand(trainerId, purchaseIds), cancellationToken);
}
