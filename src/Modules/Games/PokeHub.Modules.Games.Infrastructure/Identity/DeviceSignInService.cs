using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Application.DeviceSignIn;

namespace PokeHub.Modules.Games.Infrastructure.Identity;

/// <summary>
/// Keycloak's device flow, asked over Docker's network under Keycloak's public host name: the
/// approval page, the tokens' issuer and refreshes all then agree on one address the API trusts,
/// however the device reached PokeHub.
/// </summary>
internal sealed partial class DeviceSignInService(
    HttpClient httpClient,
    IOptions<GamesKeyCloakOptions> options,
    ILogger<DeviceSignInService> logger)
    : IDeviceSignInService
{
    // offline_access: the refresh token outlives the Keycloak session, so a console that syncs
    // now and then stays signed in.
    private const string Scope = "openid offline_access";

    public async Task<Result<DeviceSignInCode>> StartAsync(string clientId, string codeChallenge, CancellationToken cancellationToken = default)
    {
        Result<DeviceCodeResponse> response = await PostAsync<DeviceCodeResponse>("auth/device", new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["scope"] = Scope,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256",
        }, cancellationToken);

        if (response.IsFailure)
        {
            return Result.Failure<DeviceSignInCode>(response.Error);
        }

        DeviceCodeResponse code = response.Value;

        return new DeviceSignInCode(
            code.DeviceCode,
            code.UserCode,
            new Uri(ToPublic(code.VerificationUri)),
            new Uri(ToPublic(code.VerificationUriComplete)),
            code.ExpiresIn,
            code.Interval);
    }

    public Task<Result<DeviceSignInTokens>> GetTokensAsync(string clientId, string deviceCode, string codeVerifier, CancellationToken cancellationToken = default)
    {
        return GetTokensAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
            ["client_id"] = clientId,
            ["device_code"] = deviceCode,
            ["code_verifier"] = codeVerifier,
        }, cancellationToken);
    }

    public Task<Result<DeviceSignInTokens>> RefreshAsync(string clientId, string refreshToken, CancellationToken cancellationToken = default)
    {
        return GetTokensAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = clientId,
            ["refresh_token"] = refreshToken,
        }, cancellationToken);
    }

    private async Task<Result<DeviceSignInTokens>> GetTokensAsync(Dictionary<string, string> form, CancellationToken cancellationToken)
    {
        Result<TokenResponse> response = await PostAsync<TokenResponse>("token", form, cancellationToken);

        return response.IsFailure
            ? Result.Failure<DeviceSignInTokens>(response.Error)
            : new DeviceSignInTokens(response.Value.AccessToken, response.Value.RefreshToken, response.Value.ExpiresIn);
    }

    private async Task<Result<T>> PostAsync<T>(string path, Dictionary<string, string> form, CancellationToken cancellationToken)
    {
        try
        {
            using var content = new FormUrlEncodedContent(form);
            using HttpResponseMessage response = await httpClient.PostAsync(new Uri(path, UriKind.Relative), content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                T? body = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
                return body is null ? Result.Failure<T>(DeviceSignInErrors.Unavailable) : body;
            }

            OAuthError? error = await response.Content.ReadFromJsonAsync<OAuthError>(cancellationToken);

            return Result.Failure<T>(error?.Error switch
            {
                "authorization_pending" => DeviceSignInErrors.Pending,
                "slow_down" => DeviceSignInErrors.SlowDown,
                "expired_token" => DeviceSignInErrors.Expired,
                "access_denied" => DeviceSignInErrors.Denied,
                "invalid_grant" => DeviceSignInErrors.SignedOut,
                _ => LogUnexpected(error, path),
            });
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            LogRequestFailed(exception, path);
            return Result.Failure<T>(DeviceSignInErrors.Unavailable);
        }
    }

    private Error LogUnexpected(OAuthError? error, string path)
    {
        LogUnexpectedAnswer(path, error?.Error, error?.Description);
        return DeviceSignInErrors.Unavailable;
    }

    // In case Keycloak still names its internal address in the approval page's link.
    private string ToPublic(string uri)
    {
        string internalOrigin = new Uri(options.Value.OpenIdConnectUrl).GetLeftPart(UriPartial.Authority);
        string publicOrigin = options.Value.PublicUrl.TrimEnd('/');

        return string.IsNullOrEmpty(publicOrigin) || !uri.StartsWith(internalOrigin, StringComparison.Ordinal)
            ? uri
            : publicOrigin + uri[internalOrigin.Length..];
    }

    [LoggerMessage(LogLevel.Error, "Sign-in by code failed calling Keycloak's {Path}")]
    private partial void LogRequestFailed(Exception exception, string path);

    [LoggerMessage(LogLevel.Warning, "Keycloak's {Path} answered {Error}: {Description}")]
    private partial void LogUnexpectedAnswer(string path, string? error, string? description);

    private sealed record DeviceCodeResponse(
        [property: JsonPropertyName("device_code")] string DeviceCode,
        [property: JsonPropertyName("user_code")] string UserCode,
        [property: JsonPropertyName("verification_uri")] string VerificationUri,
        [property: JsonPropertyName("verification_uri_complete")] string VerificationUriComplete,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("interval")] int Interval);

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record OAuthError(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("error_description")] string? Description);
}
