using System.Security.Claims;

namespace PokeHub.Modules.Games.Presentation;

internal static class PlayerClaims
{
    /// <summary>
    /// The client the token was issued to (OpenID Connect "authorized party"). For a player
    /// who signed in from a game, it is that game's client id.
    /// </summary>
    internal static string? GetClientId(this ClaimsPrincipal user) => user.FindFirst("azp")?.Value;
}
