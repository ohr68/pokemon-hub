namespace PokeHub.Modules.Games.Presentation.Play.Contracts;

internal sealed record CatchPokemonRequest(
    int SpeciesId,
    int Level,
    bool IsShiny,
    string CatchKey,
    string? Nickname = null,
    bool FromOtherGame = false);
