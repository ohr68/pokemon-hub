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
    /// Mirrors the game's save: Pokémon the game caught that are in its save on PokeHub but no
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
}
