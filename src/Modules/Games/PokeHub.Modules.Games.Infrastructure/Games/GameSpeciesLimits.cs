using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Infrastructure.Database;
using PokeHub.Modules.Games.PublicApi;

namespace PokeHub.Modules.Games.Infrastructure.Games;

internal sealed class GameSpeciesLimits(GamesDbContext context) : IGameSpeciesLimits
{
    public async Task<IReadOnlyDictionary<Guid, int>> GetMaxSpeciesIdsAsync(
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<Game>()
            .AsNoTracking()
            .Where(g => gameIds.Contains(g.Id) && g.MaxSpeciesId != null)
            .ToDictionaryAsync(g => g.Id, g => g.MaxSpeciesId!.Value, cancellationToken);
    }
}
