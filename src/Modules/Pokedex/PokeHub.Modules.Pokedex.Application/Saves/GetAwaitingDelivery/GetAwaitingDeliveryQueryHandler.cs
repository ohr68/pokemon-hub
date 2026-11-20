using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Saves.GetAwaitingDelivery;

internal sealed class GetAwaitingDeliveryQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetAwaitingDeliveryQuery, IReadOnlyCollection<AwaitingDeliveryResponse>>
{
    public async Task<Result<IReadOnlyCollection<AwaitingDeliveryResponse>>> Handle(
        GetAwaitingDeliveryQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                p.id AS {nameof(AwaitingDeliveryResponse.Id)},
                p.species_id AS {nameof(AwaitingDeliveryResponse.SpeciesId)},
                s.name AS {nameof(AwaitingDeliveryResponse.SpeciesName)},
                p.nickname AS {nameof(AwaitingDeliveryResponse.Nickname)},
                p.level AS {nameof(AwaitingDeliveryResponse.Level)},
                p.is_shiny AS {nameof(AwaitingDeliveryResponse.IsShiny)},
                p.original_trainer_id = p.trainer_id AS {nameof(AwaitingDeliveryResponse.IsOriginalTrainer)},
                p.original_trainer_id AS {nameof(AwaitingDeliveryResponse.OriginalTrainerId)},
                t.name AS {nameof(AwaitingDeliveryResponse.OriginalTrainerName)},
                p.game_data_format AS {nameof(AwaitingDeliveryResponse.GameDataFormat)},
                p.game_data AS {nameof(AwaitingDeliveryResponse.GameData)}
             FROM pokedex.pokemon p
             JOIN pokedex.saves sv ON sv.id = p.save_id
             JOIN pokedex.species s ON s.id = p.species_id
             JOIN pokedex.trainers t ON t.id = p.original_trainer_id
             WHERE sv.trainer_id = @TrainerId
               AND sv.game_id = @GameId
               AND p.trainer_id = @TrainerId
               AND p.status = 'Active'
               AND p.awaiting_delivery
             ORDER BY p.obtained_on_utc
             """;

        IEnumerable<AwaitingDeliveryResponse> pokemon = await connection.QueryAsync<AwaitingDeliveryResponse>(
            sql,
            new { request.TrainerId, request.GameId });

        return pokemon.ToList();
    }
}
