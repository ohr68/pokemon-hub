namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public interface IOwnedPokemonRepository
{
    Task<OwnedPokemon?> GetAsync(Guid pokemonId, CancellationToken cancellationToken = default);

    Task<bool> IsTradeCompletedAsync(Guid tradeId, CancellationToken cancellationToken = default);

    void Insert(OwnedPokemon pokemon);
}
