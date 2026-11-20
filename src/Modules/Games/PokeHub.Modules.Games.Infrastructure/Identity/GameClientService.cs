using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Infrastructure.Identity;

/// <summary>
/// Creates a game's client in Keycloak: a public client (browser games can't keep secrets)
/// using the authorization code flow with PKCE, like pokemon-web's.
/// </summary>
internal sealed partial class GameClientService(HttpClient httpClient, ILogger<GameClientService> logger)
    : IGameClientService
{
    public async Task<Result> CreateClientAsync(GameClient client, CancellationToken cancellationToken = default)
    {
        var representation = new ClientRepresentation(
            client.ClientId,
            client.Name,
            Enabled: true,
            PublicClient: true,
            StandardFlowEnabled: true,
            DirectAccessGrantsEnabled: false,
            ImplicitFlowEnabled: false,
            RedirectUris: [.. client.RedirectUris],
            WebOrigins: [.. client.WebOrigins],
            Attributes: new Dictionary<string, string>
            {
                ["pkce.code.challenge.method"] = "S256",
                ["post.logout.redirect.uris"] = string.Join("##", client.RedirectUris),
            });

        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                new Uri("clients", UriKind.Relative),
                representation,
                cancellationToken);

            return Result.Success();
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            return Result.Failure(GameErrors.SlugTaken(client.ClientId));
        }
        catch (HttpRequestException exception)
        {
            LogClientCreationFailed(exception, client.ClientId);

            return Result.Failure(GameErrors.ClientNotCreated);
        }
    }

    [LoggerMessage(LogLevel.Error, "Creating the Keycloak client {ClientId} failed")]
    private partial void LogClientCreationFailed(Exception exception, string clientId);

    private sealed record ClientRepresentation(
        [property: JsonPropertyName("clientId")] string ClientId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("enabled")] bool Enabled,
        [property: JsonPropertyName("publicClient")] bool PublicClient,
        [property: JsonPropertyName("standardFlowEnabled")] bool StandardFlowEnabled,
        [property: JsonPropertyName("directAccessGrantsEnabled")] bool DirectAccessGrantsEnabled,
        [property: JsonPropertyName("implicitFlowEnabled")] bool ImplicitFlowEnabled,
        [property: JsonPropertyName("redirectUris")] IReadOnlyList<string> RedirectUris,
        [property: JsonPropertyName("webOrigins")] IReadOnlyList<string> WebOrigins,
        [property: JsonPropertyName("attributes")] IReadOnlyDictionary<string, string> Attributes);
}
