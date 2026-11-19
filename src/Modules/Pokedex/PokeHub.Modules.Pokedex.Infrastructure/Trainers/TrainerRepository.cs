using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Pokedex.Domain.Trainers;
using PokeHub.Modules.Pokedex.Infrastructure.Database;

namespace PokeHub.Modules.Pokedex.Infrastructure.Trainers;

internal sealed class TrainerRepository(PokedexDbContext context) : ITrainerRepository
{
    public async Task<Trainer?> GetAsync(Guid trainerId, CancellationToken cancellationToken = default)
    {
        return await context.Trainers.SingleOrDefaultAsync(t => t.Id == trainerId, cancellationToken);
    }

    public async Task<Trainer?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await context.Trainers.SingleOrDefaultAsync(t => t.Code == code, cancellationToken);
    }

    public void Insert(Trainer trainer)
    {
        context.Trainers.Add(trainer);
    }
}
