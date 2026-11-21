using System.Text.Json;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.Webhooks;

namespace PokeHub.Modules.Games.Application.Webhooks;

/// <summary>
/// Queues an event for the game's webhook, when it has one and is on. Sent in the background.
/// </summary>
/// <param name="EventType">pokemon.moved_in, pokemon.delivered, ping, ...</param>
/// <param name="Data">The event's details, sent as the data field.</param>
public sealed record QueueWebhookEventCommand(Guid GameId, string EventType, object Data) : ICommand<Guid?>;

internal sealed class QueueWebhookEventCommandHandler(
    IGameRepository gameRepository,
    IWebhookDeliveryRepository deliveryRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<QueueWebhookEventCommand, Guid?>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<Guid?>> Handle(QueueWebhookEventCommand request, CancellationToken cancellationToken)
    {
        Game? game = await gameRepository.GetAsync(request.GameId, cancellationToken);

        if (game is null)
        {
            return Result.Failure<Guid?>(GameErrors.NotFound(request.GameId));
        }

        if (game.WebhookUrl is null || !game.IsActive)
        {
            // Not (Guid?)null: a null value converts to a failed Result.
            return Result.Success<Guid?>(null);
        }

        var id = Guid.CreateVersion7();
        DateTime utcNow = dateTimeProvider.UtcNow;

        // The body the game receives; the id lets it ignore a delivery it already handled.
        string payload = JsonSerializer.Serialize(
            new { id, type = request.EventType, occurredOnUtc = utcNow, gameId = game.Id, data = request.Data },
            JsonOptions);

        deliveryRepository.Insert(WebhookDelivery.Create(id, game.Id, request.EventType, payload, utcNow));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return id;
    }
}
