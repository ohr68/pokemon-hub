namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;

public sealed record PokemonResponse(
    Guid Id,
    int SpeciesId,
    string SpeciesName,
    string PrimaryType,
    string? SecondaryType,
    string? Nickname,
    int Level,
    bool IsShiny,
    string Origin,
    bool IsOriginalTrainer,
    DateTime ObtainedOnUtc,
    Guid? SaveId,
    string? GameName);
