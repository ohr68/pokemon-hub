using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Infrastructure.Database;

namespace PokeHub.Modules.Pokedex.Infrastructure.Species;

internal sealed class SpeciesRepository(PokedexDbContext context) : ISpeciesRepository
{
    private const int MaxEvolutionStages = 4;

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

    public Task<bool> HasEvolutionLinesAsync(CancellationToken cancellationToken = default)
    {
        return context.Species.AnyAsync(s => s.EvolvesFromSpeciesId != null, cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetEvolutionPathAsync(
        int fromSpeciesId,
        int toSpeciesId,
        CancellationToken cancellationToken = default)
    {
        var path = new List<int>();
        int current = toSpeciesId;

        // No evolution line is longer than this; it also stops on bad data.
        for (int stage = 0; stage < MaxEvolutionStages; stage++)
        {
            path.Insert(0, current);

            int? previous = await context.Species
                .Where(s => s.Id == current)
                .Select(s => s.EvolvesFromSpeciesId)
                .SingleOrDefaultAsync(cancellationToken);

            if (previous is null)
            {
                return [];
            }

            if (previous == fromSpeciesId)
            {
                return path;
            }

            current = previous.Value;
        }

        return [];
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

    public async Task<IReadOnlyList<Item>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<Item>().ToListAsync(cancellationToken);
    }

    public Task<bool> AnyItemsAsync(CancellationToken cancellationToken = default)
    {
        return context.Set<Item>().AnyAsync(cancellationToken);
    }

    public void Insert(Item item)
    {
        context.Set<Item>().Add(item);
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
