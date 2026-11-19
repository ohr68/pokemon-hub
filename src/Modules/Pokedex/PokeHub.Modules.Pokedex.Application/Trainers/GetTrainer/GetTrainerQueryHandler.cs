using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;

internal sealed class GetTrainerQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTrainerQuery, TrainerResponse>
{
    public async Task<Result<TrainerResponse>> Handle(GetTrainerQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                id AS {nameof(TrainerResponse.Id)},
                name AS {nameof(TrainerResponse.Name)},
                code AS {nameof(TrainerResponse.Code)},
                starter_species_id AS {nameof(TrainerResponse.StarterSpeciesId)}
             FROM pokedex.trainers
             WHERE id = @TrainerId
             """;

        TrainerResponse? trainer = await connection.QuerySingleOrDefaultAsync<TrainerResponse>(sql, request);

        // A trainer appears a few seconds after registration, once the UserRegistered event arrives.
        return trainer is null
            ? Result.Failure<TrainerResponse>(TrainerErrors.NotFound(request.TrainerId))
            : trainer;
    }
}
