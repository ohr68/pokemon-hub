using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

internal sealed class GetSpeciesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetSpeciesQuery, SpeciesPageResponse>
{
    private const string Filter =
        """
        WHERE (@Search IS NULL OR name ILIKE '%' || @Search || '%' OR CAST(s.id AS text) = @Search)
          AND (@Type IS NULL OR primary_type = @Type OR secondary_type = @Type)
          AND (@Generation IS NULL OR generation = @Generation)
          AND (
            @Game::text IS NULL
            OR (@NationalMax::int IS NOT NULL AND s.id <= @NationalMax::int)
            OR (@NationalMax::int IS NULL AND e.species_id IS NOT NULL)
          )
        """;

    private const string Source =
        """
        FROM pokedex.species s
        LEFT JOIN pokedex.game_dex_entries e ON e.species_id = s.id AND e.game_slug = @Game::text
        """;

    public async Task<Result<SpeciesPageResponse>> Handle(GetSpeciesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                s.id AS {nameof(SpeciesResponse.Id)},
                name AS {nameof(SpeciesResponse.Name)},
                generation AS {nameof(SpeciesResponse.Generation)},
                primary_type AS {nameof(SpeciesResponse.PrimaryType)},
                secondary_type AS {nameof(SpeciesResponse.SecondaryType)}
             {Source}
             {Filter}
             ORDER BY CASE WHEN @NationalMax::int IS NULL THEN e.position END, s.id
             LIMIT @PageSize OFFSET @Offset;

             SELECT COUNT(*)
             {Source}
             {Filter};
             """;

        string? game = string.IsNullOrWhiteSpace(request.Game) ? null : request.Game.Trim();
        int? nationalMax = request.National && game is not null ? GameDexes.Find(game)?.NationalMaxSpeciesId : null;

        var parameters = new
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            Type = string.IsNullOrWhiteSpace(request.Type) ? null : request.Type.Trim().ToLowerInvariant(),
            request.Generation,
            Game = game,
            NationalMax = nationalMax,
            request.PageSize,
            Offset = (request.Page - 1) * request.PageSize,
        };

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, parameters);

        var items = (await results.ReadAsync<SpeciesResponse>()).ToList();
        int totalCount = await results.ReadSingleAsync<int>();

        return new SpeciesPageResponse(items, request.Page, request.PageSize, totalCount);
    }
}
