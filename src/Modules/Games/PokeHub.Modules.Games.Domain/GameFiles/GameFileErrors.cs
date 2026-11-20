using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.GameFiles;

public static class GameFileErrors
{
    public static Error NotFound(Guid fileId)
    {
        return Error.NotFound("GameFiles.NotFound", $"No game file {fileId} was found.");
    }

    public static Error TooLarge(int maxBytes)
    {
        return Error.Problem("GameFiles.TooLarge", $"The file can be at most {maxBytes / (1024 * 1024)} MB.");
    }

    public static readonly Error NotAnImage = Error.Problem(
        "GameFiles.NotAnImage",
        "Upload a PNG, JPEG, WebP or GIF image.");

    public static readonly Error Empty = Error.Problem("GameFiles.Empty", "The file is empty.");

    public static readonly Error NoRom = Error.NotFound("GameFiles.NoRom", "This game has no ROM yet.");
}
