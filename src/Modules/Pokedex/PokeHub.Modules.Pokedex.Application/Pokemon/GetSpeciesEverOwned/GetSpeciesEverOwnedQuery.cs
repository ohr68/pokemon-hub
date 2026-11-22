using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetSpeciesEverOwned;

/// <param name="TrainerId">Only this trainer; empty for all of them.</param>
public sealed record GetSpeciesEverOwnedQuery(Guid? TrainerId) : IQuery<IReadOnlyDictionary<Guid, int>>;

internal sealed class GetSpeciesEverOwnedQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetSpeciesEverOwnedQuery, IReadOnlyDictionary<Guid, int>>
{
    public async Task<Result<IReadOnlyDictionary<Guid, int>>> Handle(GetSpeciesEverOwnedQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // The history keeps every species a Pokémon was while with each trainer.
        const string sql =
            """
            SELECT trainer_id AS TrainerId, count(DISTINCT species_id)::int AS Species
            FROM pokedex.ownership_history
            WHERE @TrainerId::uuid IS NULL OR trainer_id = @TrainerId
            GROUP BY trainer_id
            """;

        IEnumerable<Row> rows = await connection.QueryAsync<Row>(sql, new { request.TrainerId });

        return rows.ToDictionary(r => r.TrainerId, r => r.Species);
    }

    private sealed record Row(Guid TrainerId, int Species);
}
