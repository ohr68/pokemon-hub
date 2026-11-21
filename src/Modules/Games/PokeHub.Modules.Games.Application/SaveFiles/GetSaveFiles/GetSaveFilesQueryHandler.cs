using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.SaveFiles.GetSaveFiles;

internal sealed class GetSaveFilesQueryHandler(IGameRepository gameRepository, IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetSaveFilesQuery, IReadOnlyCollection<SaveFileResponse>>
{
    public async Task<Result<IReadOnlyCollection<SaveFileResponse>>> Handle(
        GetSaveFilesQuery request,
        CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<SaveFileResponse>>(game.Error);
        }

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // Leaves the content out: the list only describes the backups.
        const string sql =
            $"""
             SELECT
                id AS {nameof(SaveFileResponse.Id)},
                size_bytes AS {nameof(SaveFileResponse.SizeBytes)},
                sha256 AS {nameof(SaveFileResponse.Sha256)},
                uploaded_on_utc AS {nameof(SaveFileResponse.UploadedOnUtc)},
                label AS {nameof(SaveFileResponse.Label)}
             FROM games.save_files
             WHERE trainer_id = @TrainerId AND game_id = @GameId
             ORDER BY uploaded_on_utc DESC
             """;

        IEnumerable<SaveFileResponse> saveFiles = await connection.QueryAsync<SaveFileResponse>(
            sql,
            new { request.TrainerId, GameId = game.Value.Id });

        return saveFiles.ToList();
    }
}
