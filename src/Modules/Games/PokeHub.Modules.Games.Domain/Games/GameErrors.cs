using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.Games;

public static class GameErrors
{
    public static Error SlugTaken(string slug)
    {
        return Error.Conflict("Games.SlugTaken", $"A game already uses the slug {slug}.");
    }

    public static readonly Error NotAGame = Error.Problem(
        "Games.NotAGame",
        "This request didn't come from a registered, active game. Sign in through the game.");

    public static readonly Error ClientNotCreated = Error.Problem(
        "Games.ClientNotCreated",
        "The game's sign-in client could not be created in the identity provider.");
}
