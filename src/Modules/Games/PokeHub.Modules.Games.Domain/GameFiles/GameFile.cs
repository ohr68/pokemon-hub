using System.Security.Cryptography;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.GameFiles;

/// <summary>
/// A file an admin uploaded for a game: its cover, icon, screenshots or the game itself.
/// A game has at most one cover, icon and ROM; uploading another replaces it.
/// </summary>
public sealed class GameFile : Entity
{
    public const int MaxImageSizeBytes = 5 * 1024 * 1024;

    // The largest GBA ROMs are 32 MB.
    public const int MaxRomSizeBytes = 64 * 1024 * 1024;

    public static readonly IReadOnlySet<string> ImageContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/webp", "image/gif",
    };

    private GameFile()
    {
    }

    public Guid Id { get; private set; }

    public Guid GameId { get; private set; }

    public GameFileKind Kind { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public int SizeBytes { get; private set; }

    // Hex SHA-256: lets browsers keep a cached copy until the file changes.
    public string Sha256 { get; private set; }

    public byte[] Content { get; private set; }

    public DateTime UploadedOnUtc { get; private set; }

    public bool IsSingle => Kind is not GameFileKind.Screenshot;

    public static Result<GameFile> Upload(
        Guid gameId,
        GameFileKind kind,
        string fileName,
        string contentType,
        byte[] content,
        DateTime utcNow)
    {
        if (kind is GameFileKind.Rom)
        {
            if (content.Length > MaxRomSizeBytes)
            {
                return Result.Failure<GameFile>(GameFileErrors.TooLarge(MaxRomSizeBytes));
            }

            contentType = "application/octet-stream";
        }
        else
        {
            if (!ImageContentTypes.Contains(contentType))
            {
                return Result.Failure<GameFile>(GameFileErrors.NotAnImage);
            }

            if (content.Length > MaxImageSizeBytes)
            {
                return Result.Failure<GameFile>(GameFileErrors.TooLarge(MaxImageSizeBytes));
            }
        }

        if (content.Length == 0)
        {
            return Result.Failure<GameFile>(GameFileErrors.Empty);
        }

        return new GameFile
        {
            Id = Guid.CreateVersion7(),
            GameId = gameId,
            Kind = kind,
            FileName = Path.GetFileName(fileName),
            ContentType = contentType.ToLowerInvariant(),
            SizeBytes = content.Length,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(content)),
            Content = content,
            UploadedOnUtc = utcNow,
        };
    }
}
