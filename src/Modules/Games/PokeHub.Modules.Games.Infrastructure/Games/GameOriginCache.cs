using System.Collections.Frozen;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PokeHub.Common.Application.Data;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.PublicApi;

namespace PokeHub.Modules.Games.Infrastructure.Games;

/// <summary>
/// Keeps the browser origins and rate limits of active games in memory. CORS checks and the
/// rate limiter run on every request and can't wait on the database, so they are reloaded in
/// the background every minute, and right away when a game is registered or changed.
/// </summary>
// A singleton, so it opens a scope for each refresh: the connection factory is scoped.
internal sealed partial class GameOriginCache(IServiceScopeFactory serviceScopeFactory, ILogger<GameOriginCache> logger)
    : BackgroundService, IGameOriginCache, IGameOriginPolicy, IGameRateLimits
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);

    private readonly SemaphoreSlim _refreshNow = new(0);

    private FrozenSet<string> _origins = FrozenSet<string>.Empty;

    private FrozenDictionary<string, int> _rateLimits = FrozenDictionary<string, int>.Empty;

    public bool IsAllowed(string origin) => _origins.Contains(origin);

    // A game not known yet gets the default until the next refresh.
    public int GetLimitPerMinute(string clientId) =>
        _rateLimits.GetValueOrDefault(clientId, Domain.Games.Game.DefaultRateLimitPerMinute);

    public void Invalidate() => _refreshNow.Release();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await LoadAsync();
            }
            catch (DbException exception)
            {
                // Keep the last known origins; try again on the next round.
                LogLoadFailed(exception);
            }

            try
            {
                await _refreshNow.WaitAsync(RefreshInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    public override void Dispose()
    {
        _refreshNow.Dispose();
        base.Dispose();
    }

    private async Task LoadAsync()
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        await using DbConnection connection = await scope.ServiceProvider
            .GetRequiredService<IDbConnectionFactory>()
            .OpenConnectionAsync();

        IEnumerable<string> origins = await connection.QueryAsync<string>(
            "SELECT DISTINCT unnest(web_origins) FROM games.games WHERE is_active");

        _origins = origins.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        IEnumerable<(string ClientId, int Limit)> limits = await connection.QueryAsync<(string, int)>(
            "SELECT client_id, rate_limit_per_minute FROM games.games WHERE is_active");

        _rateLimits = limits.ToFrozenDictionary(l => l.ClientId, l => l.Limit, StringComparer.Ordinal);
    }

    [LoggerMessage(LogLevel.Warning, "Loading the origins of registered games failed")]
    private partial void LogLoadFailed(Exception exception);
}
