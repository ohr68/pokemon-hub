namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

/// <summary>
/// A Pokémon as a game stored it when it left the game.
/// </summary>
/// <param name="DataFormat">The format of <paramref name="Data"/>, such as pk3 for Generation III.</param>
/// <param name="Data">The game's own bytes for the Pokémon.</param>
public sealed record GameSnapshot(int SpeciesId, int Level, string? Nickname, string DataFormat, byte[] Data);
