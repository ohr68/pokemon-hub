using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Games.GetGames;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.Games.GetGame;

internal sealed class GetGameQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetGameQuery, GameDetailsResponse>
{
    public async Task<Result<GameDetailsResponse>> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        GameRow? game = await connection.QuerySingleOrDefaultAsync<GameRow>(
            GameRow.Sql + " WHERE g.slug = @Slug AND g.is_active",
            new { request.Slug });

        if (game is null)
        {
            return Result.Failure<GameDetailsResponse>(GameErrors.NotFound(request.Slug));
        }

        IEnumerable<ScreenshotRow> screenshots = await connection.QueryAsync<ScreenshotRow>(
            """
            SELECT id AS Id, sha256 AS Sha256
            FROM games.game_files
            WHERE game_id = @Id AND kind = 'Screenshot'
            ORDER BY uploaded_on_utc
            """,
            new { game.Id });

        GameResponse summary = game.ToResponse();

        return new GameDetailsResponse(
            summary.Id,
            summary.Name,
            summary.Slug,
            summary.Description,
            summary.Url,
            summary.ClientId,
            summary.IconUrl,
            summary.CoverUrl,
            screenshots.Select(s => GameFileUrls.For(s.Id, s.Sha256)!).ToList(),
            game.RomId is null ? null : new RomResponse(game.RomId.Value, game.RomFileName!, game.RomSizeBytes!.Value, game.RomSha256!));
    }

    private sealed record ScreenshotRow(Guid Id, string Sha256);
}
