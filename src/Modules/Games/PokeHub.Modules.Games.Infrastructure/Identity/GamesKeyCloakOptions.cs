namespace PokeHub.Modules.Games.Infrastructure.Identity;

// The Games module's own settings for Keycloak's admin API, under Games:KeyCloak.
internal sealed class GamesKeyCloakOptions
{
    public string AdminUrl { get; init; } = string.Empty;

    public string TokenUrl { get; init; } = string.Empty;

    public string ConfidentialClientId { get; init; } = string.Empty;

    public string ConfidentialClientSecret { get; init; } = string.Empty;
}
