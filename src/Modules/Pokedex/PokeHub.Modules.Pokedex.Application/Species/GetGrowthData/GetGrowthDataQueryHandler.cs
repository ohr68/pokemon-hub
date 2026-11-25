using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Species.GetGrowthData;

internal sealed class GetGrowthDataQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetGrowthDataQuery, GrowthDataResponse>
{
    public async Task<Result<GrowthDataResponse>> Handle(GetGrowthDataQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT id AS SpeciesId, name AS Name, growth_rate AS GrowthRate
            FROM pokedex.species
            ORDER BY id;

            SELECT name AS Name, experience_by_level AS ExperienceByLevel
            FROM pokedex.growth_rates;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql);

        var species = (await results.ReadAsync<GrowthSpeciesResponse>()).ToList();
        var growthRates = (await results.ReadAsync<GrowthRateRow>()).ToDictionary(r => r.Name, r => r.ExperienceByLevel);

        return new GrowthDataResponse(species, growthRates);
    }

    // A class with setters: Dapper hands PostgreSQL arrays over as Array, which doesn't match a
    // constructor's int[] parameter but is assigned to an int[] property.
    private sealed class GrowthRateRow
    {
        public string Name { get; init; } = string.Empty;

        public int[] ExperienceByLevel { get; init; } = [];
    }
}
