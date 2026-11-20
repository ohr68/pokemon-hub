namespace PokeHub.Modules.Pokedex.PublicApi;

/// <param name="CatchKey">The game's identifier for this catch; repeating it returns the same Pokémon.</param>
public sealed record CatchRequest(
    Guid TrainerId,
    Guid GameId,
    string GameName,
    int SpeciesId,
    int Level,
    bool IsShiny,
    string CatchKey);
