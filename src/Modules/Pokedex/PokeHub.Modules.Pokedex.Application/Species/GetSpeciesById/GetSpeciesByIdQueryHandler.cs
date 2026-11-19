using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Species.GetSpecies;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Species.GetSpeciesById;

internal sealed class GetSpeciesByIdQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetSpeciesByIdQuery, SpeciesResponse>
{
    public async Task<Result<SpeciesResponse>> Handle(GetSpeciesByIdQuery request, CancellationToken cancellationToken)
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
             WHERE id = @SpeciesId
             """;

        SpeciesResponse? species = await connection.QuerySingleOrDefaultAsync<SpeciesResponse>(sql, request);

        return species is null
            ? Result.Failure<SpeciesResponse>(SpeciesErrors.NotFound(request.SpeciesId))
            : species;
    }
}
