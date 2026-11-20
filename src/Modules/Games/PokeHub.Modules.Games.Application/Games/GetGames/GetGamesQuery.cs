using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.Games.GetGames;

/// <summary>
/// The active games players can open.
/// </summary>
public sealed record GetGamesQuery : IQuery<IReadOnlyCollection<GameResponse>>;

/// <param name="IconUrl">Relative to the API root, when the game has an icon.</param>
/// <param name="HasRom">The game runs from a ROM PokeHub keeps, in pokemon-web's emulator.</param>
public sealed record GameResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Uri Url,
    string ClientId,
    Uri? IconUrl,
    Uri? CoverUrl,
    bool HasRom);
