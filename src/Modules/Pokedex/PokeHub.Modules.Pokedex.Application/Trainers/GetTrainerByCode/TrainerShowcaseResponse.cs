using PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;

namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainerByCode;

public sealed record TrainerShowcaseResponse(
    Guid Id,
    string Name,
    string Code,
    IReadOnlyCollection<PokemonResponse> Pokemon);
