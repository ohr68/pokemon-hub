using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Infrastructure.Database;

namespace PokeHub.Modules.Pokedex.Infrastructure.Species;

internal sealed class SpeciesRepository(PokedexDbContext context) : ISpeciesRepository
{
    public async Task<IReadOnlyList<PokemonSpecies>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Species.Include(s => s.Gen3LevelUpMoves).ToListAsync(cancellationToken);
    }

    public async Task<PokemonSpecies?> GetAsync(int speciesId, CancellationToken cancellationToken = default)
    {
        return await context.Species.SingleOrDefaultAsync(s => s.Id == speciesId, cancellationToken);
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

    public async Task<IReadOnlyList<GrowthRate>> GetGrowthRatesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<GrowthRate>().ToListAsync(cancellationToken);
    }

    public void Insert(GrowthRate growthRate)
    {
        context.Set<GrowthRate>().Add(growthRate);
    }

    public async Task<IReadOnlyList<Move>> GetMovesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<Move>().ToListAsync(cancellationToken);
    }

    public void Insert(Move move)
    {
        context.Set<Move>().Add(move);
    }

    public Task<bool> AnyGameDexEntriesAsync(CancellationToken cancellationToken = default)
    {
        return context.Set<GameDexEntry>().AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>> GetIdsAsync(CancellationToken cancellationToken = default)
    {
        return await context.Species.Select(s => s.Id).ToListAsync(cancellationToken);
    }

    public async Task ReplaceGameDexEntriesAsync(
        IReadOnlyCollection<GameDexEntry> entries,
        CancellationToken cancellationToken = default)
    {
        await context.Set<GameDexEntry>().ExecuteDeleteAsync(cancellationToken);

        context.Set<GameDexEntry>().AddRange(entries);
    }
}
