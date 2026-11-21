using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>
/// What other modules may ask of the Pokédex, in process. The Pokédex stays the only owner
/// of Pokémon: other modules read through here and ask it to make changes.
/// </summary>
public interface IPokedexApi
{
    Task<PokemonInfo?> GetPokemonAsync(Guid pokemonId, CancellationToken cancellationToken = default);

    Task<TrainerInfo?> GetTrainerAsync(Guid trainerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Swaps two Pokémon between their trainers in one transaction, evolving those that evolve
    /// by trading. Safe to repeat for the same trade.
    /// </summary>
    Task<Result> TradePokemonAsync(TradeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// The trainer's save for a game, created on first use.
    /// </summary>
    Task<Result<SaveInfo>> GetOrCreateSaveAsync(
        Guid trainerId,
        Guid gameId,
        string gameName,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<SavePokemonInfo>>> GetSavePokemonAsync(
        Guid trainerId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a Pokémon caught in a game, into the trainer's save for it. Repeating the same
    /// catch key returns the same Pokémon.
    /// </summary>
    Task<Result<Guid>> CatchPokemonAsync(CatchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mirrors the game's save: Pokémon inside the game (caught or delivered) that are in its save on PokeHub but no
    /// longer among <paramref name="presentCatchKeys"/> left the game and are removed. With
    /// <paramref name="dryRun"/>, only reports them.
    /// </summary>
    Task<Result<IReadOnlyCollection<LeftGameInfo>>> ReconcileSaveAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<string> presentCatchKeys,
        bool dryRun,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a Pokémon that is in the trainer's save for this game.
    /// </summary>
    Task<Result> ReleaseFromSaveAsync(
        Guid trainerId,
        Guid gameId,
        Guid pokemonId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pokémon the trainer moved into their save for this game on PokeHub, which the game
    /// still has to write into its own save.
    /// </summary>
    Task<Result<IReadOnlyCollection<DeliveryInfo>>> GetAwaitingDeliveryAsync(
        Guid trainerId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The game wrote these Pokémon into its own save. All are recorded, or none.
    /// </summary>
    Task<Result> DeliverPokemonAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<DeliveredPokemon> deliveries,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The game took these Pokémon out of its own save; they move to the hub. All move, or none.
    /// </summary>
    Task<Result<IReadOnlyCollection<Guid>>> WithdrawPokemonAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<WithdrawnPokemon> withdrawals,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gives a trainer a Pokémon bought in the shop, in the hub. Safe to repeat for the same
    /// purchase: the Pokémon gets the purchase's id.
    /// </summary>
    Task<Result<Guid>> GiveBoughtPokemonAsync(
        Guid purchaseId,
        Guid trainerId,
        int speciesId,
        int level,
        bool isShiny,
        CancellationToken cancellationToken = default);

    /// <summary>A species in the catalog, or null when there is none with that number.</summary>
    Task<SpeciesInfo?> GetSpeciesAsync(int speciesId, CancellationToken cancellationToken = default);
}
