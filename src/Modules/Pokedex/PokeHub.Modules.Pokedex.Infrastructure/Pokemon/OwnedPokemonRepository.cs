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
            p => p.CaughtInGameId == gameId && p.CatchKey == catchKey,
            cancellationToken);
    }

    public void Insert(OwnedPokemon pokemon)
    {
        context.Pokemon.Add(pokemon);
    }
}
