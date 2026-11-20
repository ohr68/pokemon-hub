namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>
/// A Pokémon waiting to be written into a game's own save.
/// </summary>
/// <param name="IsOriginalTrainer">The player is the Pokémon's original trainer.</param>
/// <param name="GameData">The Pokémon as the last game that held it stored it, in <paramref name="GameDataFormat"/>.</param>
public sealed record DeliveryInfo(
    Guid Id,
    int SpeciesId,
    string SpeciesName,
    string? Nickname,
    int Level,
    bool IsShiny,
    bool IsOriginalTrainer,
    Guid OriginalTrainerId,
    string OriginalTrainerName,
    string? GameDataFormat,
    byte[]? GameData);

/// <param name="CatchKey">How the game knows the Pokémon now.</param>
public sealed record DeliveredPokemon(Guid PokemonId, string CatchKey);

/// <param name="SpeciesId">Its national number now; it may have evolved in the game.</param>
/// <param name="Data">The game's own bytes for the Pokémon, in <paramref name="DataFormat"/>.</param>
public sealed record WithdrawnPokemon(
    string CatchKey,
    int SpeciesId,
    int Level,
    string? Nickname,
    string DataFormat,
    byte[] Data);
