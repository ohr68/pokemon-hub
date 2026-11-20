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

    public static Error NotFound(Guid gameId)
    {
        return Error.NotFound("Games.NotFound", $"No game {gameId} was found.");
    }

    public static Error NotFound(string slug)
    {
        return Error.NotFound("Games.NotFound", $"No game {slug} was found.");
    }

    public static readonly Error NoWebhook = Error.Problem(
        "Games.NoWebhook",
        "The game has no webhook. Set its URL first.");

    public static readonly Error ClientNotUpdated = Error.Problem(
        "Games.ClientNotUpdated",
        "The game's sign-in client could not be updated in the identity provider.");
}
