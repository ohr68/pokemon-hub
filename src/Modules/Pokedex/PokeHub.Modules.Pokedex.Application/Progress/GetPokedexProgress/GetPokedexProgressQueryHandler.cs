using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Progress.GetPokedexProgress;

// Progress comes from the ownership history: a species counts as caught once the trainer has
// obtained one, and releasing it later doesn't remove it from the Pokédex (like the games).
internal sealed class GetPokedexProgressQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPokedexProgressQuery, PokedexProgressResponse>
{
    public async Task<Result<PokedexProgressResponse>> Handle(
        GetPokedexProgressQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT DISTINCT species_id
            FROM pokedex.ownership_history
            WHERE trainer_id = @TrainerId AND change = 'Obtained'
            ORDER BY species_id;

            SELECT COUNT(*) FROM pokedex.species;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, request);

        var caughtSpeciesIds = (await results.ReadAsync<int>()).ToList();
        int total = await results.ReadSingleAsync<int>();

        return new PokedexProgressResponse(caughtSpeciesIds.Count, total, caughtSpeciesIds);
    }
}
