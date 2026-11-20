using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Saves.GetMySaves;

internal sealed class GetMySavesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMySavesQuery, IReadOnlyCollection<SaveResponse>>
{
    public async Task<Result<IReadOnlyCollection<SaveResponse>>> Handle(
        GetMySavesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                s.id AS {nameof(SaveResponse.Id)},
                s.game_id AS {nameof(SaveResponse.GameId)},
                s.game_name AS {nameof(SaveResponse.GameName)},
                s.created_on_utc AS {nameof(SaveResponse.CreatedOnUtc)},
                COUNT(p.id)::int AS {nameof(SaveResponse.PokemonCount)}
             FROM pokedex.saves s
             LEFT JOIN pokedex.pokemon p ON p.save_id = s.id AND p.status = 'Active'
             WHERE s.trainer_id = @TrainerId
             GROUP BY s.id
             ORDER BY s.created_on_utc
             """;

        IEnumerable<SaveResponse> saves = await connection.QueryAsync<SaveResponse>(sql, request);

        return saves.ToList();
    }
}
