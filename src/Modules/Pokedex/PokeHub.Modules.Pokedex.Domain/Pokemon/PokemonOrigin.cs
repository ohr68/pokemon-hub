namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

// How the first trainer got the Pokémon; a trade doesn't change it.
public enum PokemonOrigin
{
    Starter = 1,
    Grant = 2,

    // Caught in a game connected to PokeHub.
    Game = 3,
}
