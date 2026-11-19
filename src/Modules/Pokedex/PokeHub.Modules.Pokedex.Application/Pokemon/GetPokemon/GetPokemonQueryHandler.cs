using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Domain.Pokemon;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetPokemon;

internal sealed class GetPokemonQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPokemonQuery, PokemonDetailsResponse>
{
    public async Task<Result<PokemonDetailsResponse>> Handle(GetPokemonQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                p.id AS {nameof(PokemonDetailsResponse.Id)},
                p.trainer_id AS {nameof(PokemonDetailsResponse.TrainerId)},
                p.species_id AS {nameof(PokemonDetailsResponse.SpeciesId)},
                s.name AS {nameof(PokemonDetailsResponse.SpeciesName)},
                p.nickname AS {nameof(PokemonDetailsResponse.Nickname)},
                p.level AS {nameof(PokemonDetailsResponse.Level)},
                p.is_shiny AS {nameof(PokemonDetailsResponse.IsShiny)},
                p.status = 'Active' AS {nameof(PokemonDetailsResponse.IsActive)}
             FROM pokedex.pokemon p
             JOIN pokedex.species s ON s.id = p.species_id
             WHERE p.id = @PokemonId
             """;

        PokemonDetailsResponse? pokemon = await connection.QuerySingleOrDefaultAsync<PokemonDetailsResponse>(sql, request);

        return pokemon is null
            ? Result.Failure<PokemonDetailsResponse>(PokemonErrors.NotFound(request.PokemonId))
            : pokemon;
    }
}
