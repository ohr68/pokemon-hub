using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;
using PokeHub.Modules.Pokedex.Domain.Pokemon;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetMyPokemonDetails;

public sealed record GetMyPokemonDetailsQuery(Guid TrainerId, Guid PokemonId) : IQuery<MyPokemonDetailsResponse>;

public sealed record MyPokemonDetailsResponse(PokemonResponse Pokemon, IReadOnlyCollection<PokemonGameResponse> Games);

/// <param name="IsHere">The Pokémon is in this game's save now.</param>
/// <param name="Level">Its level there at the last sync; empty if never reported.</param>
/// <param name="BattleData">JSON, from the last sync; empty if never reported.</param>
public sealed record PokemonGameResponse(
    Guid GameId,
    string GameName,
    bool IsHere,
    bool IsAwaitingDelivery,
    int? Level,
    string? BattleData,
    DateTime? UpdatedOnUtc);

internal sealed class GetMyPokemonDetailsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMyPokemonDetailsQuery, MyPokemonDetailsResponse>
{
    public async Task<Result<MyPokemonDetailsResponse>> Handle(GetMyPokemonDetailsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string pokemonSql =
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
                sv.game_name AS {nameof(PokemonResponse.GameName)},
                p.awaiting_delivery AS {nameof(PokemonResponse.IsAwaitingDelivery)},
                (p.save_id IS NOT NULL AND NOT p.awaiting_delivery AND p.catch_key IS NOT NULL) AS {nameof(PokemonResponse.IsInsideGame)}
             FROM pokedex.pokemon p
             JOIN pokedex.species s ON s.id = p.species_id
             LEFT JOIN pokedex.saves sv ON sv.id = p.save_id
             WHERE p.id = @PokemonId AND p.trainer_id = @TrainerId AND p.status = 'Active'
             """;

        PokemonResponse? pokemon = await connection.QuerySingleOrDefaultAsync<PokemonResponse>(pokemonSql, request);

        if (pokemon is null)
        {
            return Result.Failure<MyPokemonDetailsResponse>(PokemonErrors.NotFound(request.PokemonId));
        }

        // Every game of this trainer it has been in, the one it's in now first.
        const string gamesSql =
            $"""
             SELECT
                sv.game_id AS {nameof(PokemonGameResponse.GameId)},
                sv.game_name AS {nameof(PokemonGameResponse.GameName)},
                COALESCE(p.save_id = sv.id, false) AS {nameof(PokemonGameResponse.IsHere)},
                (COALESCE(p.save_id = sv.id, false) AND p.awaiting_delivery) AS {nameof(PokemonGameResponse.IsAwaitingDelivery)},
                st.level AS {nameof(PokemonGameResponse.Level)},
                st.battle_data::text AS {nameof(PokemonGameResponse.BattleData)},
                st.updated_on_utc AS {nameof(PokemonGameResponse.UpdatedOnUtc)}
             FROM pokedex.saves sv
             JOIN pokedex.pokemon p ON p.id = @PokemonId
             LEFT JOIN pokedex.pokemon_game_status st ON st.pokemon_id = p.id AND st.game_id = sv.game_id
             WHERE sv.trainer_id = @TrainerId
               AND (p.save_id = sv.id
                    OR st.pokemon_id IS NOT NULL
                    OR EXISTS (SELECT 1 FROM pokedex.ownership_history h WHERE h.pokemon_id = p.id AND h.save_id = sv.id))
             ORDER BY 3 DESC, st.updated_on_utc DESC NULLS LAST, sv.game_name
             """;

        IEnumerable<PokemonGameResponse> games = await connection.QueryAsync<PokemonGameResponse>(gamesSql, request);

        return new MyPokemonDetailsResponse(pokemon, games.ToList());
    }
}
