using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Application.Games.GetGames;

internal sealed class GetGamesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetGamesQuery, IReadOnlyCollection<GameResponse>>
{
    public async Task<Result<IReadOnlyCollection<GameResponse>>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        IEnumerable<GameRow> games = await connection.QueryAsync<GameRow>(GameRow.Sql + " WHERE g.is_active ORDER BY g.name");

        return games.Select(g => g.ToResponse()).ToList();
    }
}

/// <summary>A game with its icon, cover and ROM, as the lists read it.</summary>
internal sealed record GameRow(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string Url,
    string ClientId,
    bool IsActive,
    int RateLimitPerMinute,
    string? WebhookUrl,
    Guid? IconId,
    string? IconSha256,
    Guid? CoverId,
    string? CoverSha256,
    Guid? RomId,
    string? RomFileName,
    int? RomSizeBytes,
    string? RomSha256)
{
    internal const string Sql =
        """
        SELECT
            g.id AS Id, g.name AS Name, g.slug AS Slug, g.description AS Description, g.url AS Url,
            g.client_id AS ClientId, g.is_active AS IsActive, g.rate_limit_per_minute AS RateLimitPerMinute,
            g.webhook_url AS WebhookUrl,
            icon.id AS IconId, icon.sha256 AS IconSha256,
            cover.id AS CoverId, cover.sha256 AS CoverSha256,
            rom.id AS RomId, rom.file_name AS RomFileName, rom.size_bytes AS RomSizeBytes, rom.sha256 AS RomSha256
        FROM games.games g
        LEFT JOIN games.game_files icon ON icon.game_id = g.id AND icon.kind = 'Icon'
        LEFT JOIN games.game_files cover ON cover.game_id = g.id AND cover.kind = 'Cover'
        LEFT JOIN games.game_files rom ON rom.game_id = g.id AND rom.kind = 'Rom'
        """;

    internal GameResponse ToResponse() => new(
        Id,
        Name,
        Slug,
        Description,
        new Uri(Url),
        ClientId,
        GameFileUrls.For(IconId, IconSha256),
        GameFileUrls.For(CoverId, CoverSha256),
        RomId is not null);
}
