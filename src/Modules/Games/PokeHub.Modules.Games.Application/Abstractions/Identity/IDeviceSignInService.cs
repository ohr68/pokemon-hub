using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Application.Abstractions.Identity;

/// <summary>
/// Sign-in by code (the OAuth device flow) for a game's client, for devices without a browser,
/// like the 3DS sync app: the device shows a code, the player approves it on another device,
/// and the device then gets the game's tokens. Asked of the identity provider on PokeHub's
/// behalf, so the tokens are issued as the API expects whatever address the device used.
/// </summary>
public interface IDeviceSignInService
{
    /// <param name="CodeChallenge">The device's PKCE challenge (S256), which the game clients require.</param>
    Task<Result<DeviceSignInCode>> StartAsync(string clientId, string codeChallenge, CancellationToken cancellationToken = default);

    Task<Result<DeviceSignInTokens>> GetTokensAsync(string clientId, string deviceCode, string codeVerifier, CancellationToken cancellationToken = default);

    Task<Result<DeviceSignInTokens>> RefreshAsync(string clientId, string refreshToken, CancellationToken cancellationToken = default);
}

/// <param name="DeviceCode">Secret: the device keeps it to ask for its tokens.</param>
/// <param name="UserCode">What the player types or sees on the approval page.</param>
/// <param name="VerificationUri">The approval page, where the player can type the code.</param>
/// <param name="VerificationUriComplete">The approval page with the code filled in.</param>
/// <param name="Interval">Seconds to wait between asks for the tokens.</param>
public sealed record DeviceSignInCode(
    string DeviceCode,
    string UserCode,
    Uri VerificationUri,
    Uri VerificationUriComplete,
    int ExpiresIn,
    int Interval);

/// <param name="RefreshToken">A long-lived (offline) one, so the device stays signed in between syncs.</param>
public sealed record DeviceSignInTokens(string AccessToken, string RefreshToken, int ExpiresIn);
