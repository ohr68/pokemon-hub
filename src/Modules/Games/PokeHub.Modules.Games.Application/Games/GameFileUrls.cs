namespace PokeHub.Modules.Games.Application.Games;

/// <summary>
/// Where a game's image is served, relative to the API root. The hash in the query string
/// changes with the file, so browsers can cache each version for good.
/// </summary>
public static class GameFileUrls
{
    public static Uri? For(Guid? fileId, string? sha256) =>
        fileId is null || sha256 is null ? null : new Uri($"games/files/{fileId}?v={sha256[..12]}", UriKind.Relative);
}
