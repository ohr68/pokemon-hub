using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Infrastructure.Database;
using PokeHub.Modules.Games.PublicApi;

namespace PokeHub.Modules.Games.Infrastructure.Stats;

internal sealed class PlayerGameCountersReader(GamesDbContext context) : IPlayerGameCounters
{
    public async Task<IReadOnlyCollection<PlayerGameCounters>> GetCountersAsync(Guid? trainerId = null, CancellationToken cancellationToken = default)
    {
        var rows = await context.PlayerGameStats
            .AsNoTracking()
            .Where(s => trainerId == null || s.TrainerId == trainerId)
            .Select(s => new { s.TrainerId, s.GameId, s.Counters })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new PlayerGameCounters(
                r.TrainerId,
                r.GameId,
                JsonSerializer.Deserialize<Dictionary<string, long>>(r.Counters) ?? []))
            .ToList();
    }

    // The admin form suggests slugs from the game's name, so "ruby" may be registered as
    // pokemon-ruby. The exact slug wins when both exist.
    private const string SuggestedPrefix = "pokemon-";

    public async Task<IReadOnlyDictionary<string, Guid>> GetGameIdsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken = default)
    {
        string[] candidates = [.. slugs, .. slugs.Select(s => SuggestedPrefix + s)];

        var games = await context.Set<Game>()
            .AsNoTracking()
            .Where(g => candidates.Contains(g.Slug))
            .Select(g => new { g.Slug, g.Id })
            .ToListAsync(cancellationToken);

        var gameIds = new Dictionary<string, Guid>(StringComparer.Ordinal);

        foreach (string slug in slugs)
        {
            var game = games.Find(g => g.Slug == slug) ?? games.Find(g => g.Slug == SuggestedPrefix + slug);

            if (game is not null)
            {
                gameIds[slug] = game.Id;
            }
        }

        return gameIds;
    }
}
