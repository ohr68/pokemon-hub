using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Games.Domain.SaveEdits;
using PokeHub.Modules.Games.Infrastructure.Database;

namespace PokeHub.Modules.Games.Infrastructure.SaveEdits;

internal sealed class SaveEditRepository(GamesDbContext context) : ISaveEditRepository
{
    public async Task<IReadOnlyList<SaveEdit>> GetOpenAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return await context.SaveEdits
            .Where(s => s.TrainerId == trainerId && s.GameId == gameId && s.SettledOnUtc == null)
            .OrderBy(s => s.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public void Insert(SaveEdit saveEdit)
    {
        context.SaveEdits.Add(saveEdit);
    }
}
