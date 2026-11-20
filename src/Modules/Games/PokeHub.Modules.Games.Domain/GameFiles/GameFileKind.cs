namespace PokeHub.Modules.Games.Domain.GameFiles;

public enum GameFileKind
{
    // Shown on the game's card in the games list.
    Cover = 1,

    // A small square image: the game's card and its play page.
    Icon = 2,

    // Pictures of the game, any number.
    Screenshot = 3,

    // The game itself (a ROM), for games that run in pokemon-web's emulator.
    Rom = 4,
}
