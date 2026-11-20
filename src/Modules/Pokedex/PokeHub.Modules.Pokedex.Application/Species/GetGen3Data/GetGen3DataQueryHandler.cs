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
            """
            SELECT
               gen3index AS Gen3Index,
               id AS SpeciesId,
               name AS Name,
               growth_rate AS GrowthRate
            FROM pokedex.species
            WHERE gen3index IS NOT NULL
            ORDER BY gen3index;

            SELECT name AS Name, experience_by_level AS ExperienceByLevel
            FROM pokedex.growth_rates;

            SELECT species_id AS SpeciesId, level AS Level, move_id AS MoveId
            FROM pokedex.species_gen3_level_up_moves
            ORDER BY species_id, "order";

            SELECT id AS Id, name AS Name, pp AS Pp
            FROM pokedex.moves
            ORDER BY id;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql);

        var species = (await results.ReadAsync<SpeciesRow>()).ToList();
        var growthRates = (await results.ReadAsync<GrowthRateRow>()).ToDictionary(r => r.Name, r => r.ExperienceByLevel);
        ILookup<int, LevelUpMoveRow> learnsets = (await results.ReadAsync<LevelUpMoveRow>()).ToLookup(m => m.SpeciesId);
        var moves = (await results.ReadAsync<Gen3MoveResponse>()).ToList();

        return new Gen3DataResponse(
            species
                .Select(s => new Gen3SpeciesResponse(
                    s.Gen3Index,
                    s.SpeciesId,
                    s.Name,
                    s.GrowthRate,
                    learnsets[s.SpeciesId].Select(m => new Gen3LevelUpMoveResponse(m.Level, m.MoveId)).ToList()))
                .ToList(),
            growthRates,
            moves);
    }

    private sealed record SpeciesRow(int Gen3Index, int SpeciesId, string Name, string? GrowthRate);

    private sealed record LevelUpMoveRow(int SpeciesId, int Level, int MoveId);

    // A class with setters: Dapper hands PostgreSQL arrays over as Array, which doesn't match a
    // constructor's int[] parameter but is assigned to an int[] property.
    private sealed class GrowthRateRow
    {
        public string Name { get; init; } = string.Empty;

        public int[] ExperienceByLevel { get; init; } = [];
    }
}
