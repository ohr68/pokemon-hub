using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.Games.ManageGames;

/// <summary>Every game, or one game when <paramref name="GameId"/> is set, for the admin pages.</summary>
public sealed record GetManagedGamesQuery(Guid? GameId = null) : IQuery<IReadOnlyCollection<ManagedGameResponse>>;

internal sealed class GetManagedGamesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetManagedGamesQuery, IReadOnlyCollection<ManagedGameResponse>>
{
    public async Task<Result<IReadOnlyCollection<ManagedGameResponse>>> Handle(
        GetManagedGamesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT
                id AS Id, name AS Name, slug AS Slug, description AS Description, url AS Url,
                client_id AS ClientId, is_active AS IsActive, rate_limit_per_minute AS RateLimitPerMinute,
                webhook_url AS WebhookUrl, webhook_secret AS WebhookSecret
            FROM games.games
            WHERE @GameId::uuid IS NULL OR id = @GameId
            ORDER BY name;

            SELECT
                id AS Id, game_id AS GameId, kind AS Kind, file_name AS FileName, content_type AS ContentType,
                size_bytes AS SizeBytes, sha256 AS Sha256, uploaded_on_utc AS UploadedOnUtc
            FROM games.game_files
            WHERE @GameId::uuid IS NULL OR game_id = @GameId
            ORDER BY uploaded_on_utc;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.GameId });

        var games = (await results.ReadAsync<GameRow>()).ToList();
        ILookup<Guid, FileRow> files = (await results.ReadAsync<FileRow>()).ToLookup(f => f.GameId);

        if (request.GameId is not null && games.Count == 0)
        {
            return Result.Failure<IReadOnlyCollection<ManagedGameResponse>>(GameErrors.NotFound(request.GameId.Value));
        }

        return games
            .Select(g => new ManagedGameResponse(
                g.Id,
                g.Name,
                g.Slug,
                g.Description,
                new Uri(g.Url),
                g.ClientId,
                g.IsActive,
                g.RateLimitPerMinute,
                g.WebhookUrl is null ? null : new Uri(g.WebhookUrl),
                g.WebhookSecret,
                files[g.Id]
                    .Select(f => new ManagedGameFileResponse(
                        f.Id,
                        f.Kind,
                        f.FileName,
                        f.ContentType,
                        f.SizeBytes,
                        f.Sha256,
                        f.UploadedOnUtc,
                        f.Kind == "Rom" ? null : GameFileUrls.For(f.Id, f.Sha256)))
                    .ToList()))
            .ToList();
    }

    private sealed record GameRow(
        Guid Id,
        string Name,
        string Slug,
        string? Description,
        string Url,
        string ClientId,
        bool IsActive,
        int RateLimitPerMinute,
        string? WebhookUrl,
        string? WebhookSecret);

    private sealed record FileRow(
        Guid Id,
        Guid GameId,
        string Kind,
        string FileName,
        string ContentType,
        int SizeBytes,
        string Sha256,
        DateTime UploadedOnUtc);
}
