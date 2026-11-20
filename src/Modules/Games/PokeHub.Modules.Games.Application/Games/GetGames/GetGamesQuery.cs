using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.Games.GetGames;

/// <summary>
/// The active games players can open.
/// </summary>
public sealed record GetGamesQuery : IQuery<IReadOnlyCollection<GameResponse>>;

public sealed record GameResponse(Guid Id, string Name, string Slug, Uri Url, string ClientId);
