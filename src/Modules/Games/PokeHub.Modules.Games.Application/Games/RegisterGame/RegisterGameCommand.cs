using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.Games.RegisterGame;

/// <param name="Slug">Short url-friendly name, used in the client id game-{slug}.</param>
/// <param name="RedirectUris">Where players come back after signing in, e.g. https://my-game.example/*.</param>
/// <param name="WebOrigins">Origins the game runs on, e.g. https://my-game.example.</param>
public sealed record RegisterGameCommand(
    string Name,
    string Slug,
    Uri Url,
    IReadOnlyCollection<string> RedirectUris,
    IReadOnlyCollection<string> WebOrigins) : ICommand<RegisteredGameResponse>;

/// <param name="ClientId">The client id the game uses to sign players in.</param>
public sealed record RegisteredGameResponse(Guid Id, string ClientId);
