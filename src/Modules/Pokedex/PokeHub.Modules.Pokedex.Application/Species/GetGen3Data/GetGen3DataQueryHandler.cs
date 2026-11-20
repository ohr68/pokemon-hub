using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Species.GetGen3Data;

internal sealed class GetGen3DataQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetGen3DataQuery, Gen3DataResponse>
{
    public async Task<Result<Gen3DataResponse>> Handle(GetGen3DataQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // EF's snake-case naming writes Gen3Index as gen3index (no underscore before a digit).
        const string sql =
            $"""
             SELECT
                gen3index AS {nameof(Gen3SpeciesResponse.Gen3Index)},
                id AS {nameof(Gen3SpeciesResponse.SpeciesId)},
                name AS {nameof(Gen3SpeciesResponse.Name)},
                growth_rate AS {nameof(Gen3SpeciesResponse.GrowthRate)}
             FROM pokedex.species
             WHERE gen3index IS NOT NULL
             ORDER BY gen3index;

             SELECT name AS Name, experience_by_level AS ExperienceByLevel
             FROM pokedex.growth_rates;
             """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql);

        var species = (await results.ReadAsync<Gen3SpeciesResponse>()).ToList();
        var growthRates = (await results.ReadAsync<GrowthRateRow>()).ToDictionary(r => r.Name, r => r.ExperienceByLevel);

        return new Gen3DataResponse(species, growthRates);
    }

    // A class with setters: Dapper hands PostgreSQL arrays over as Array, which doesn't match a
    // constructor's int[] parameter but is assigned to an int[] property.
    private sealed class GrowthRateRow
    {
        public string Name { get; init; } = string.Empty;

        public int[] ExperienceByLevel { get; init; } = [];
    }
}
