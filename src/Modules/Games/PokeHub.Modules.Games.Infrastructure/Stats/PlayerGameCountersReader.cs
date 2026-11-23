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

    public async Task<IReadOnlyDictionary<string, Guid>> GetGameIdsAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken = default)
    {
        return await context.Set<Game>()
            .AsNoTracking()
            .Where(g => slugs.Contains(g.Slug))
            .ToDictionaryAsync(g => g.Slug, g => g.Id, cancellationToken);
    }
}
