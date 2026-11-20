namespace PokeHub.Modules.Pokedex.PublicApi;

public sealed record SavePokemonInfo(
    Guid Id,
    int SpeciesId,
    string SpeciesName,
    string? Nickname,
    int Level,
    bool IsShiny,
    bool IsOriginalTrainer);
