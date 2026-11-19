using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public static class PokemonErrors
{
    public static Error NotFound(Guid pokemonId)
    {
        return Error.NotFound("Pokemon.NotFound", $"The Pokémon with the identifier {pokemonId} was not found.");
    }

    public static Error NotOwnedByTrainer(Guid pokemonId)
    {
        return Error.Problem("Pokemon.NotOwnedByTrainer", $"The Pokémon {pokemonId} belongs to another trainer.");
    }

    public static Error NotActive(Guid pokemonId)
    {
        return Error.Conflict("Pokemon.NotActive", $"The Pokémon {pokemonId} has already left its trainer.");
    }

    public static Error InvalidLevel(int level)
    {
        return Error.Problem(
            "Pokemon.InvalidLevel",
            $"The level {level} must be between {OwnedPokemon.MinLevel} and {OwnedPokemon.MaxLevel}.");
    }
}
