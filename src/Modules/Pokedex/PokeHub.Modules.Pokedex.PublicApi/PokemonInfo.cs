namespace PokeHub.Modules.Pokedex.PublicApi;

public sealed record PokemonInfo(
    Guid Id,
    Guid TrainerId,
    int SpeciesId,
    string SpeciesName,
    string? Nickname,
    int Level,
    bool IsShiny,
    bool IsActive,
    bool IsInHub);
