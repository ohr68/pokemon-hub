using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.CatchPokemon;

/// <param name="CatchKey">The game's identifier for this catch; repeating it returns the same Pokémon.</param>
/// <param name="Nickname">The name the player gave it in the game, if any.</param>
/// <param name="FromOtherGame">The save says another game caught it. Then it is only recorded if PokeHub already put it in this game, so a save from another game can't pay rewards as new catches.</param>
public sealed record CatchPokemonCommand(
    Guid TrainerId,
    Guid GameId,
    string GameName,
    int SpeciesId,
    int Level,
    bool IsShiny,
    string CatchKey,
    string? Nickname = null,
    bool FromOtherGame = false) : ICommand<Guid>;
