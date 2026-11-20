using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Infrastructure.Database;

namespace PokeHub.Modules.Pokedex.Infrastructure.Pokemon;

internal sealed class OwnedPokemonRepository(PokedexDbContext context) : IOwnedPokemonRepository
{
    public async Task<OwnedPokemon?> GetAsync(Guid pokemonId, CancellationToken cancellationToken = default)
    {
        return await context.Pokemon.SingleOrDefaultAsync(p => p.Id == pokemonId, cancellationToken);
    }

    public Task<bool> IsTradeCompletedAsync(Guid tradeId, CancellationToken cancellationToken = default)
    {
        return context.Set<OwnershipRecord>().AnyAsync(r => r.TradeId == tradeId, cancellationToken);
    }

    public async Task<OwnedPokemon?> GetByCatchKeyAsync(
        Guid gameId,
        string catchKey,
        CancellationToken cancellationToken = default)
    {
        return await context.Pokemon.SingleOrDefaultAsync(
            p => p.CatchKeyGameId == gameId && p.CatchKey == catchKey,
            cancellationToken);
    }

    public async Task<IReadOnlyList<OwnedPokemon>> GetInsideGameAsync(
        Guid saveId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return await context.Pokemon
            .Where(p => p.SaveId == saveId
                        && p.Status == PokemonStatus.Active
                        && !p.AwaitingDelivery
                        && p.CatchKeyGameId == gameId
                        && p.CatchKey != null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OwnedPokemon>> GetAwaitingDeliveryAsync(
        Guid saveId,
        CancellationToken cancellationToken = default)
    {
        return await context.Pokemon
            .Where(p => p.SaveId == saveId && p.Status == PokemonStatus.Active && p.AwaitingDelivery)
            .ToListAsync(cancellationToken);
    }

    public void Insert(OwnedPokemon pokemon)
    {
        context.Pokemon.Add(pokemon);
    }
}
