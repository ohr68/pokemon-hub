using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.ReconcileSave;

/// <summary>
/// Mirrors a game's save: Pokémon the game caught that are still in its save on PokeHub, but
/// whose catch key is no longer in <paramref name="PresentCatchKeys"/>, left the game and are
/// removed. Pokémon moved in from the hub, or out to it, are never touched.
/// </summary>
/// <param name="PresentCatchKeys">The catch keys of every Pokémon in the game's save now.</param>
/// <param name="DryRun">Only report which Pokémon would be removed.</param>
public sealed record ReconcileSaveCommand(
    Guid TrainerId,
    Guid GameId,
    IReadOnlyCollection<string> PresentCatchKeys,
    bool DryRun) : ICommand<IReadOnlyCollection<LeftGameResponse>>;

public sealed record LeftGameResponse(Guid Id, int SpeciesId, string? Nickname, int Level, bool IsShiny);
