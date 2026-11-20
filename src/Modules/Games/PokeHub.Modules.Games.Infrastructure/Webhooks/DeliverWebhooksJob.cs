using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PokeHub.Common.Application.Clock;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.Webhooks;
using PokeHub.Modules.Games.Infrastructure.Database;
using Quartz;

namespace PokeHub.Modules.Games.Infrastructure.Webhooks;

/// <summary>
/// Sends queued webhook events to the games. Each request carries:
/// - PokeHub-Event: the event type; PokeHub-Delivery: its id, the same on every retry;
/// - PokeHub-Signature: t={unix seconds},v1={hex HMAC-SHA256 of "{t}.{body}" with the game's
///   webhook secret}, so the game can check the request came from PokeHub, unchanged and recent.
/// A 2xx answer counts as delivered; anything else is retried later.
/// </summary>
[DisallowConcurrentExecution]
internal sealed partial class DeliverWebhooksJob(
    GamesDbContext dbContext,
    IHttpClientFactory httpClientFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<DeliverWebhooksJob> logger) : IJob
{
    internal const string HttpClientName = "PokeHub.Webhooks";

    private const int BatchSize = 20;

    public async Task Execute(IJobExecutionContext context)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;

        List<WebhookDelivery> due = await dbContext.WebhookDeliveries
            .Where(d => d.NextAttemptOnUtc != null && d.NextAttemptOnUtc <= utcNow)
            .OrderBy(d => d.NextAttemptOnUtc)
            .Take(BatchSize)
            .ToListAsync(context.CancellationToken);

        if (due.Count == 0)
        {
            return;
        }

        var gameIds = due.Select(d => d.GameId).Distinct().ToList();
        Dictionary<Guid, Game> games = await dbContext.Games
            .Where(g => gameIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, context.CancellationToken);

        HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);

        foreach (WebhookDelivery delivery in due)
        {
            Game game = games[delivery.GameId];

            if (game.WebhookUrl is null || game.WebhookSecret is null || !game.IsActive)
            {
                delivery.GiveUp("The game's webhook was removed or the game was turned off.");
                continue;
            }

            await SendAsync(httpClient, game, delivery, context.CancellationToken);
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }

    private async Task SendAsync(HttpClient httpClient, Game game, WebhookDelivery delivery, CancellationToken cancellationToken)
    {
        string timestamp = dateTimeProvider.UtcNow.ToUnixTimeSecondsString();

        using var request = new HttpRequestMessage(HttpMethod.Post, game.WebhookUrl);
        request.Content = new StringContent(delivery.Payload, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        request.Headers.Add("PokeHub-Event", delivery.EventType);
        request.Headers.Add("PokeHub-Delivery", delivery.Id.ToString());
        request.Headers.Add("PokeHub-Signature", $"t={timestamp},v1={Sign(game.WebhookSecret!, timestamp, delivery.Payload)}");

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                delivery.Succeeded((int)response.StatusCode, dateTimeProvider.UtcNow);
            }
            else
            {
                delivery.Failed((int)response.StatusCode, $"The game answered {(int)response.StatusCode}.", dateTimeProvider.UtcNow);
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            LogDeliveryFailed(exception, delivery.Id, game.Slug);
            delivery.Failed(null, exception.Message, dateTimeProvider.UtcNow);
        }
    }

    internal static string Sign(string secret, string timestamp, string body)
    {
        byte[] signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{body}"));
        return Convert.ToHexStringLower(signature);
    }

    [LoggerMessage(LogLevel.Warning, "Sending webhook delivery {DeliveryId} to game {GameSlug} failed")]
    private partial void LogDeliveryFailed(Exception exception, Guid deliveryId, string gameSlug);
}

internal static class UnixTime
{
    internal static string ToUnixTimeSecondsString(this DateTime utc) =>
        new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
}
