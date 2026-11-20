using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;

internal sealed class GetTrainerPokemonQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTrainerPokemonQuery, IReadOnlyCollection<PokemonResponse>>
{
    public async Task<Result<IReadOnlyCollection<PokemonResponse>>> Handle(
        GetTrainerPokemonQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                p.id AS {nameof(PokemonResponse.Id)},
                p.species_id AS {nameof(PokemonResponse.SpeciesId)},
                s.name AS {nameof(PokemonResponse.SpeciesName)},
                s.primary_type AS {nameof(PokemonResponse.PrimaryType)},
                s.secondary_type AS {nameof(PokemonResponse.SecondaryType)},
                p.nickname AS {nameof(PokemonResponse.Nickname)},
                p.level AS {nameof(PokemonResponse.Level)},
                p.is_shiny AS {nameof(PokemonResponse.IsShiny)},
                p.origin AS {nameof(PokemonResponse.Origin)},
                p.original_trainer_id = p.trainer_id AS {nameof(PokemonResponse.IsOriginalTrainer)},
                p.obtained_on_utc AS {nameof(PokemonResponse.ObtainedOnUtc)},
                p.save_id AS {nameof(PokemonResponse.SaveId)},
                sv.game_name AS {nameof(PokemonResponse.GameName)}
             FROM pokedex.pokemon p
             JOIN pokedex.species s ON s.id = p.species_id
             LEFT JOIN pokedex.saves sv ON sv.id = p.save_id
             WHERE p.trainer_id = @TrainerId AND p.status = 'Active'
               AND (NOT @HubOnly OR p.save_id IS NULL)
               AND (@GameId IS NULL OR sv.game_id = @GameId)
             ORDER BY p.species_id, p.obtained_on_utc
             """;

        IEnumerable<PokemonResponse> pokemon = await connection.QueryAsync<PokemonResponse>(sql, request);

        return pokemon.ToList();
    }
}
