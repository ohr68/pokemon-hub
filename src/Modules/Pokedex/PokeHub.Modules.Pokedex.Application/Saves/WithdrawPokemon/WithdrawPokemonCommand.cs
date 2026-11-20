using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.WithdrawPokemon;

/// <summary>
/// The game took Pokémon out of its own save and sends them to the hub. All of them move, or none.
/// </summary>
public sealed record WithdrawPokemonCommand(Guid TrainerId, Guid GameId, IReadOnlyCollection<Withdrawal> Withdrawals)
    : ICommand<IReadOnlyCollection<Guid>>;

/// <param name="CatchKey">How the game knows the Pokémon.</param>
/// <param name="SpeciesId">Its national number now; it may have evolved in the game.</param>
/// <param name="DataFormat">The format of <paramref name="Data"/>, such as pk3.</param>
/// <param name="Data">The game's own bytes for the Pokémon, written back if it returns.</param>
public sealed record Withdrawal(
    string CatchKey,
    int SpeciesId,
    int Level,
    string? Nickname,
    string DataFormat,
    byte[] Data);
