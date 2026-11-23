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
                t.id AS {nameof(TrainerResponse.Id)},
                t.name AS {nameof(TrainerResponse.Name)},
                t.code AS {nameof(TrainerResponse.Code)},
                t.starter_species_id AS {nameof(TrainerResponse.StarterSpeciesId)},
                p.id AS {nameof(TrainerResponse.AvatarPokemonId)},
                p.species_id AS {nameof(TrainerResponse.AvatarSpeciesId)},
                p.is_shiny AS {nameof(TrainerResponse.AvatarIsShiny)}
             FROM pokedex.trainers t
             LEFT JOIN pokedex.pokemon p
                ON p.id = t.avatar_pokemon_id AND p.trainer_id = t.id AND p.status = 'Active'
             WHERE t.id = @TrainerId
             """;

        TrainerResponse? trainer = await connection.QuerySingleOrDefaultAsync<TrainerResponse>(sql, request);

        // A trainer appears a few seconds after registration, once the UserRegistered event arrives.
        return trainer is null
            ? Result.Failure<TrainerResponse>(TrainerErrors.NotFound(request.TrainerId))
            : trainer;
    }
}
