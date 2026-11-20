using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.Games.GetGame;

/// <summary>
/// An active game's page: its details, screenshots and ROM, by slug.
/// </summary>
public sealed record GetGameQuery(string Slug) : IQuery<GameDetailsResponse>;

/// <param name="Rom">The game's ROM, when it has one; download it from games/{slug}/rom.</param>
public sealed record GameDetailsResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Uri Url,
    string ClientId,
    Uri? IconUrl,
    Uri? CoverUrl,
    IReadOnlyCollection<Uri> ScreenshotUrls,
    RomResponse? Rom);

/// <param name="Sha256">Tells a browser its cached copy is still the current one.</param>
public sealed record RomResponse(Guid Id, string FileName, int SizeBytes, string Sha256);
