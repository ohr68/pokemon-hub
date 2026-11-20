namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>
/// A Pokémon removed (or, in a dry run, to be removed) because it is no longer in the game.
/// </summary>
public sealed record LeftGameInfo(Guid Id, int SpeciesId, string? Nickname, int Level, bool IsShiny);
