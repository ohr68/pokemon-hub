using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.GameFiles;

namespace PokeHub.Modules.Games.Application.GameFiles;

/// <summary>
/// A game image by id (anyone may see those), or an active game's ROM by slug (signed-in
/// players only, checked by the endpoint).
/// </summary>
public sealed record GetGameFileContentQuery(Guid? ImageId = null, string? RomOfSlug = null) : IQuery<GameFileContentResponse>;

public sealed record GameFileContentResponse(string FileName, string ContentType, string Sha256, byte[] Content);

internal sealed class GetGameFileContentQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetGameFileContentQuery, GameFileContentResponse>
{
    public async Task<Result<GameFileContentResponse>> Handle(GetGameFileContentQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT f.file_name AS FileName, f.content_type AS ContentType, f.sha256 AS Sha256, f.content AS Content
            FROM games.game_files f
            JOIN games.games g ON g.id = f.game_id
            WHERE (@ImageId::uuid IS NOT NULL AND f.id = @ImageId AND f.kind <> 'Rom')
               OR (@RomOfSlug::text IS NOT NULL AND g.slug = @RomOfSlug AND g.is_active AND f.kind = 'Rom')
            LIMIT 1
            """;

        GameFileContentResponse? file = await connection.QuerySingleOrDefaultAsync<GameFileContentResponse>(
            sql,
            new { request.ImageId, request.RomOfSlug });

        if (file is null)
        {
            return Result.Failure<GameFileContentResponse>(
                request.ImageId is { } id ? GameFileErrors.NotFound(id) : GameFileErrors.NoRom);
        }

        return file;
    }
}
