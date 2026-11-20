using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Application.Abstractions.Identity;

/// <summary>
/// Creates the identity-provider client a game's players sign in with.
/// </summary>
public interface IGameClientService
{
    Task<Result> CreateClientAsync(GameClient client, CancellationToken cancellationToken = default);

    /// <summary>
    /// A disabled client can't sign anyone in, and its tokens stop being refreshed.
    /// </summary>
    Task<Result> SetEnabledAsync(string clientId, bool enabled, CancellationToken cancellationToken = default);
}

/// <param name="RedirectUris">Where the identity provider may send players back after sign-in.</param>
/// <param name="WebOrigins">Origins the game runs on.</param>
public sealed record GameClient(
    string ClientId,
    string Name,
    IReadOnlyCollection<string> RedirectUris,
    IReadOnlyCollection<string> WebOrigins);
