using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.Webhooks;

/// <summary>
/// One event to send to a game's webhook. Sent in the background and retried with growing
/// waits until the game answers with a success status, or until it gives up.
/// </summary>
public sealed class WebhookDelivery : Entity
{
    public const int MaxAttempts = 8;

    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromSeconds(30);

    private WebhookDelivery()
    {
    }

    public Guid Id { get; private set; }

    public Guid GameId { get; private set; }

    // pokemon.moved_in, pokemon.delivered, ...
    public string EventType { get; private set; }

    // The JSON body sent, as-is.
    public string Payload { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public int Attempts { get; private set; }

    public DateTime? NextAttemptOnUtc { get; private set; }

    public DateTime? DeliveredOnUtc { get; private set; }

    public int? LastStatusCode { get; private set; }

    public string? LastError { get; private set; }

    public static WebhookDelivery Create(Guid id, Guid gameId, string eventType, string payload, DateTime utcNow)
    {
        return new WebhookDelivery
        {
            Id = id,
            GameId = gameId,
            EventType = eventType,
            Payload = payload,
            CreatedOnUtc = utcNow,
            NextAttemptOnUtc = utcNow,
        };
    }

    public void Succeeded(int statusCode, DateTime utcNow)
    {
        Attempts++;
        LastStatusCode = statusCode;
        LastError = null;
        DeliveredOnUtc = utcNow;
        NextAttemptOnUtc = null;
    }

    /// <summary>
    /// Waits 30 seconds after the first failure, doubling each time (about an hour in all);
    /// after <see cref="MaxAttempts"/> it stops.
    /// </summary>
    public void Failed(int? statusCode, string error, DateTime utcNow)
    {
        Attempts++;
        LastStatusCode = statusCode;
        LastError = error.Length > 500 ? error[..500] : error;
        NextAttemptOnUtc = Attempts >= MaxAttempts
            ? null
            : utcNow + FirstRetryDelay * Math.Pow(2, Attempts - 1);
    }

    /// <summary>Stops trying, without counting an attempt (the game no longer has a webhook).</summary>
    public void GiveUp(string reason)
    {
        LastError = reason;
        NextAttemptOnUtc = null;
    }
}
