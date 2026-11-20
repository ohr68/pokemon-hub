using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveFiles;

namespace PokeHub.Modules.Games.Application.SaveFiles.GetSaveFileContent;

internal sealed class GetSaveFileContentQueryHandler(IGameRepository gameRepository, IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetSaveFileContentQuery, SaveFileContentResponse>
{
    public async Task<Result<SaveFileContentResponse>> Handle(GetSaveFileContentQuery request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure<SaveFileContentResponse>(game.Error);
        }

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // Only the player's own backups for this game, so an id from elsewhere finds nothing.
        const string sql =
            """
            SELECT id AS Id, uploaded_on_utc AS UploadedOnUtc, content AS Content
            FROM games.save_files
            WHERE trainer_id = @TrainerId
              AND game_id = @GameId
              AND (@SaveFileId::uuid IS NULL OR id = @SaveFileId)
            ORDER BY uploaded_on_utc DESC
            LIMIT 1
            """;

        SaveFileRow? row = await connection.QuerySingleOrDefaultAsync<SaveFileRow>(
            sql,
            new { request.TrainerId, GameId = game.Value.Id, request.SaveFileId });

        if (row is null)
        {
            return Result.Failure<SaveFileContentResponse>(request.SaveFileId is { } id ? SaveFileErrors.NotFound(id) : SaveFileErrors.NoneYet);
        }

        return new SaveFileContentResponse(row.Id, game.Value.Slug, row.UploadedOnUtc, row.Content);
    }

    private sealed record SaveFileRow(Guid Id, DateTime UploadedOnUtc, byte[] Content);
}
