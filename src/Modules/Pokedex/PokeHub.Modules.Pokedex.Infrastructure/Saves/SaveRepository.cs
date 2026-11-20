using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Pokedex.Domain.Saves;
using PokeHub.Modules.Pokedex.Infrastructure.Database;

namespace PokeHub.Modules.Pokedex.Infrastructure.Saves;

internal sealed class SaveRepository(PokedexDbContext context) : ISaveRepository
{
    public async Task<Save?> GetAsync(Guid saveId, CancellationToken cancellationToken = default)
    {
        return await context.Saves.SingleOrDefaultAsync(s => s.Id == saveId, cancellationToken);
    }

    public async Task<Save?> GetForGameAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return await context.Saves.SingleOrDefaultAsync(s => s.TrainerId == trainerId && s.GameId == gameId, cancellationToken);
    }

    public void Insert(Save save)
    {
        context.Saves.Add(save);
    }
}
