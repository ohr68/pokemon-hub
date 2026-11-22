namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public interface IOwnedPokemonRepository
{
    Task<OwnedPokemon?> GetAsync(Guid pokemonId, CancellationToken cancellationToken = default);

    Task<bool> IsTradeCompletedAsync(Guid tradeId, CancellationToken cancellationToken = default);

    Task<OwnedPokemon?> GetByCatchKeyAsync(Guid gameId, string catchKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Active Pokémon in the save that are inside the game's own save: caught there or
    /// delivered into it, so the game knows them by their catch key.
    /// </summary>
    Task<IReadOnlyList<OwnedPokemon>> GetInsideGameAsync(Guid saveId, Guid gameId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OwnedPokemon>> GetAwaitingDeliveryAsync(Guid saveId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PokemonGameStatus>> GetGameStatusesAsync(
        Guid gameId,
        IReadOnlyCollection<Guid> pokemonIds,
        CancellationToken cancellationToken = default);

    void Insert(PokemonGameStatus status);

    void Insert(OwnedPokemon pokemon);
}
