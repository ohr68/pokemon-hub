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

        const string sql =
            $"""
             SELECT
                id AS {nameof(GameResponse.Id)},
                name AS {nameof(GameResponse.Name)},
                slug AS {nameof(GameResponse.Slug)},
                url AS {nameof(GameResponse.Url)},
                client_id AS {nameof(GameResponse.ClientId)}
             FROM games.games
             WHERE is_active
             ORDER BY name
             """;

        IEnumerable<GameRow> games = await connection.QueryAsync<GameRow>(sql);

        return games.Select(g => new GameResponse(g.Id, g.Name, g.Slug, new Uri(g.Url), g.ClientId)).ToList();
    }

    // The url column is text; Dapper reads it as a string first.
    private sealed record GameRow(Guid Id, string Name, string Slug, string Url, string ClientId);
}
