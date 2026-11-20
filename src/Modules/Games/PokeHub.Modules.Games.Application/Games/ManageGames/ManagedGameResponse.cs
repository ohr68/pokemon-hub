namespace PokeHub.Modules.Games.Application.Games.ManageGames;

/// <summary>
/// A game as admins see it: turned off games too, with its settings and files.
/// </summary>
public sealed record ManagedGameResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Uri Url,
    string ClientId,
    bool IsActive,
    int RateLimitPerMinute,
    Uri? WebhookUrl,
    string? WebhookSecret,
    IReadOnlyCollection<ManagedGameFileResponse> Files);

/// <param name="Url">Where an image is served, relative to the API root; empty for the ROM.</param>
public sealed record ManagedGameFileResponse(
    Guid Id,
    string Kind,
    string FileName,
    string ContentType,
    int SizeBytes,
    string Sha256,
    DateTime UploadedOnUtc,
    Uri? Url);
