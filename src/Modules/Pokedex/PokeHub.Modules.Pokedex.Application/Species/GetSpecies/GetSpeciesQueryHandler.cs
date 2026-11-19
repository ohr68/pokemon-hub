using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

internal sealed class GetSpeciesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetSpeciesQuery, SpeciesPageResponse>
{
    private const string Filter =
        """
        WHERE (@Search IS NULL OR name ILIKE '%' || @Search || '%' OR CAST(id AS text) = @Search)
          AND (@Type IS NULL OR primary_type = @Type OR secondary_type = @Type)
          AND (@Generation IS NULL OR generation = @Generation)
        """;

    public async Task<Result<SpeciesPageResponse>> Handle(GetSpeciesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                id AS {nameof(SpeciesResponse.Id)},
                name AS {nameof(SpeciesResponse.Name)},
                generation AS {nameof(SpeciesResponse.Generation)},
                primary_type AS {nameof(SpeciesResponse.PrimaryType)},
                secondary_type AS {nameof(SpeciesResponse.SecondaryType)}
             FROM pokedex.species
             {Filter}
             ORDER BY id
             LIMIT @PageSize OFFSET @Offset;

             SELECT COUNT(*) FROM pokedex.species
             {Filter};
             """;

        var parameters = new
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            Type = string.IsNullOrWhiteSpace(request.Type) ? null : request.Type.Trim().ToLowerInvariant(),
            request.Generation,
            request.PageSize,
            Offset = (request.Page - 1) * request.PageSize,
        };

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, parameters);

        var items = (await results.ReadAsync<SpeciesResponse>()).ToList();
        int totalCount = await results.ReadSingleAsync<int>();

        return new SpeciesPageResponse(items, request.Page, request.PageSize, totalCount);
    }
}
