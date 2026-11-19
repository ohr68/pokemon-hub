using MediatR;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainerByCode;

internal sealed class GetTrainerByCodeQueryHandler(ITrainerRepository trainerRepository, ISender sender)
    : IQueryHandler<GetTrainerByCodeQuery, TrainerShowcaseResponse>
{
    public async Task<Result<TrainerShowcaseResponse>> Handle(
        GetTrainerByCodeQuery request,
        CancellationToken cancellationToken)
    {
        string code = TrainerCode.Normalize(request.Code);

        Trainer? trainer = await trainerRepository.GetByCodeAsync(code, cancellationToken);

        if (trainer is null)
        {
            return Result.Failure<TrainerShowcaseResponse>(TrainerErrors.CodeNotFound(code));
        }

        Result<IReadOnlyCollection<PokemonResponse>> pokemon = await sender.Send(
            new GetTrainerPokemonQuery(trainer.Id),
            cancellationToken);

        if (pokemon.IsFailure)
        {
            return Result.Failure<TrainerShowcaseResponse>(pokemon.Error);
        }

        return new TrainerShowcaseResponse(trainer.Id, trainer.Name, trainer.Code, pokemon.Value);
    }
}
