using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.CancelWithdrawal;

/// <summary>
/// The game recorded a withdrawal but couldn't take the Pokémon out of its save (the page
/// closed, or the restart failed). They go back inside the game. All of them, or none.
/// </summary>
/// <param name="CatchKeys">The Pokémon to put back, by how the game knows them.</param>
/// <param name="PresentCatchKeys">The catch keys of every Pokémon in the game's save now.</param>
public sealed record CancelWithdrawalCommand(
    Guid TrainerId,
    Guid GameId,
    IReadOnlyCollection<string> CatchKeys,
    IReadOnlyCollection<string> PresentCatchKeys) : ICommand;
