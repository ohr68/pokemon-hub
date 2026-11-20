using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.CatchPokemon;

/// <param name="CatchKey">The game's identifier for this catch; repeating it returns the same Pokémon.</param>
public sealed record CatchPokemonCommand(
    Guid TrainerId,
    Guid GameId,
    string GameName,
    int SpeciesId,
    int Level,
    bool IsShiny,
    string CatchKey) : ICommand<Guid>;
