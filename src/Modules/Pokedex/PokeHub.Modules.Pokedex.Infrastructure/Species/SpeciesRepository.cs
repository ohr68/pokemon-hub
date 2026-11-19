using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Infrastructure.Database;

namespace PokeHub.Modules.Pokedex.Infrastructure.Species;

internal sealed class SpeciesRepository(PokedexDbContext context) : ISpeciesRepository
{
    public async Task<IReadOnlyList<PokemonSpecies>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Species.ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(int speciesId, CancellationToken cancellationToken = default)
    {
        return context.Species.AnyAsync(s => s.Id == speciesId, cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return context.Species.AnyAsync(cancellationToken);
    }

    public void Insert(PokemonSpecies species)
    {
        context.Species.Add(species);
    }
}
