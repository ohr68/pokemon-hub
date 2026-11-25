namespace PokeHub.Modules.Games.Infrastructure.Identity;

// The Games module's own settings for Keycloak (its admin API, and sign-in by code), under
// Games:KeyCloak.
internal sealed class GamesKeyCloakOptions
{
    public string AdminUrl { get; init; } = string.Empty;

    public string TokenUrl { get; init; } = string.Empty;

    public string ConfidentialClientId { get; init; } = string.Empty;

    public string ConfidentialClientSecret { get; init; } = string.Empty;

    // The realm's OpenID Connect endpoints as the API reaches them (inside Docker), for
    // sign-in by code: tokens asked for here are issued as the API expects.
    public string OpenIdConnectUrl { get; init; } = string.Empty;

    // Keycloak's address as players reach it, to show them the page where they approve a code.
    public string PublicUrl { get; init; } = string.Empty;
}
