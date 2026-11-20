using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PokeHub.Modules.Games.PublicApi;

namespace PokeHub.Api.Extensions;

internal static class RateLimitingExtensions
{
    /// <summary>
    /// The game API's limit: each player gets the game's own number of requests per minute,
    /// counted separately per game. Requests over it get 429 Too Many Requests with Retry-After.
    /// </summary>
    internal static IServiceCollection AddGameRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(
                        System.Globalization.CultureInfo.InvariantCulture);
                }

                await Results.Problem(
                        title: "Games.TooManyRequests",
                        detail: "Too many requests from this game. Wait a moment and try again.",
                        statusCode: StatusCodes.Status429TooManyRequests)
                    .ExecuteAsync(context.HttpContext);
            };

            options.AddPolicy(GameRateLimiting.PolicyName, context =>
            {
                string clientId = context.User.FindFirst("azp")?.Value ?? "unknown";
                string player = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? context.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown";
                int limit = context.RequestServices.GetRequiredService<IGameRateLimits>().GetLimitPerMinute(clientId);

                // The limit is part of the key, so a changed limit takes effect right away.
                return RateLimitPartition.GetFixedWindowLimiter(
                    $"{clientId}:{player}:{limit}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    });
            });
        });

        return services;
    }
}
