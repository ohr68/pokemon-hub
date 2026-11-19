namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetPokemon;

public sealed record PokemonDetailsResponse(
    Guid Id,
    Guid TrainerId,
    int SpeciesId,
    string SpeciesName,
    string? Nickname,
    int Level,
    bool IsShiny,
    bool IsActive);
