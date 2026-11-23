using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetGameFacts;

/// <param name="TrainerId">Only this trainer; empty for all of them.</param>
public sealed record GetGameFactsQuery(Guid? TrainerId) : IQuery<IReadOnlyCollection<GameFactResponse>>;

/// <param name="IsEvolution">It evolved in the game; otherwise it was caught there.</param>
/// <param name="SpeciesId">The species it was caught as, or evolved into.</param>
public sealed record GameFactResponse(
    Guid TrainerId,
    Guid GameId,
    bool IsEvolution,
    Guid PokemonId,
    int SpeciesId,
    bool IsShiny,
    DateTime OccurredOnUtc);

// The ownership history already says in which save each catch and evolution happened, so
// past ones count without any backfill.
internal sealed class GetGameFactsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetGameFactsQuery, IReadOnlyCollection<GameFactResponse>>
{
    public async Task<Result<IReadOnlyCollection<GameFactResponse>>> Handle(
        GetGameFactsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                h.trainer_id AS {nameof(GameFactResponse.TrainerId)},
                s.game_id AS {nameof(GameFactResponse.GameId)},
                h.change = 'EvolvedInGame' AS {nameof(GameFactResponse.IsEvolution)},
                h.pokemon_id AS {nameof(GameFactResponse.PokemonId)},
                h.species_id AS {nameof(GameFactResponse.SpeciesId)},
                p.is_shiny AS {nameof(GameFactResponse.IsShiny)},
                h.occurred_on_utc AS {nameof(GameFactResponse.OccurredOnUtc)}
             FROM pokedex.ownership_history h
             JOIN pokedex.saves s ON s.id = h.save_id
             JOIN pokedex.pokemon p ON p.id = h.pokemon_id
             WHERE h.change IN ('Obtained', 'EvolvedInGame')
               AND (@TrainerId::uuid IS NULL OR h.trainer_id = @TrainerId)
             ORDER BY h.occurred_on_utc
             """;

        return (await connection.QueryAsync<GameFactResponse>(sql, new { request.TrainerId })).ToList();
    }
}
