using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Infrastructure.Database;

namespace PokeHub.Modules.Games.Infrastructure.Games;

internal sealed class GameRepository(GamesDbContext context) : IGameRepository
{
    public async Task<Game?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return await context.Games.SingleOrDefaultAsync(g => g.ClientId == clientId, cancellationToken);
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return context.Games.AnyAsync(g => g.Slug == slug, cancellationToken);
    }

    public void Insert(Game game)
    {
        context.Games.Add(game);
    }
}
