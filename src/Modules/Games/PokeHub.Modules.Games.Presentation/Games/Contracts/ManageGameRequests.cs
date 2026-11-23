using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Games.Presentation.Games.Contracts;

internal sealed class GameRouteRequest
{
    public Guid GameId { get; init; }
}

internal sealed class GameSlugRequest
{
    public string Slug { get; init; } = string.Empty;
}

internal sealed class GameFileRouteRequest
{
    public Guid GameId { get; init; }

    public Guid FileId { get; init; }
}

internal sealed class UpdateGameRequest
{
    public Guid GameId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int RateLimitPerMinute { get; init; }

    // The last National Dex number in the game; empty when it takes any Pokémon.
    public int? MaxSpeciesId { get; init; }
}

/// <summary>Multipart/form-data with the file in a field named file.</summary>
internal sealed class UploadGameFileRequest
{
    public Guid GameId { get; init; }

    // cover, icon, rom or screenshots.
    public string Kind { get; init; } = string.Empty;

    public IFormFile File { get; init; } = null!;
}
