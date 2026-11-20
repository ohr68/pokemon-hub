namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public interface IOwnedPokemonRepository
{
    Task<OwnedPokemon?> GetAsync(Guid pokemonId, CancellationToken cancellationToken = default);

    Task<bool> IsTradeCompletedAsync(Guid tradeId, CancellationToken cancellationToken = default);

    Task<OwnedPokemon?> GetByCatchKeyAsync(Guid gameId, string catchKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Active Pokémon in the save that were caught by the save's game (they have a catch key).
    /// </summary>
    Task<IReadOnlyList<OwnedPokemon>> GetCaughtInSaveAsync(Guid saveId, Guid gameId, CancellationToken cancellationToken = default);

    void Insert(OwnedPokemon pokemon);
}
