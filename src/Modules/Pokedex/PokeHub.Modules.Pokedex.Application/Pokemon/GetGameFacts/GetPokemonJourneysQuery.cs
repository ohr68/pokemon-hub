using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetGameFacts;

/// <param name="TrainerId">Only this trainer; empty for all of them.</param>
public sealed record GetPokemonJourneysQuery(Guid? TrainerId) : IQuery<IReadOnlyCollection<PokemonJourneyStepResponse>>;

public sealed record PokemonJourneyStepResponse(Guid TrainerId, Guid PokemonId, string Change, Guid? GameId, DateTime OccurredOnUtc);

internal sealed class GetPokemonJourneysQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPokemonJourneysQuery, IReadOnlyCollection<PokemonJourneyStepResponse>>
{
    public async Task<Result<IReadOnlyCollection<PokemonJourneyStepResponse>>> Handle(
        GetPokemonJourneysQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                h.trainer_id AS {nameof(PokemonJourneyStepResponse.TrainerId)},
                h.pokemon_id AS {nameof(PokemonJourneyStepResponse.PokemonId)},
                h.change AS {nameof(PokemonJourneyStepResponse.Change)},
                s.game_id AS {nameof(PokemonJourneyStepResponse.GameId)},
                h.occurred_on_utc AS {nameof(PokemonJourneyStepResponse.OccurredOnUtc)}
             FROM pokedex.ownership_history h
             LEFT JOIN pokedex.saves s ON s.id = h.save_id
             WHERE @TrainerId::uuid IS NULL OR h.trainer_id = @TrainerId
             ORDER BY h.occurred_on_utc
             """;

        return (await connection.QueryAsync<PokemonJourneyStepResponse>(sql, new { request.TrainerId })).ToList();
    }
}
