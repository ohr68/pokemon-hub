using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Trainers.ChooseStarter;

internal sealed class ChooseStarterCommandHandler(
    ITrainerRepository trainerRepository,
    ISpeciesRepository speciesRepository,
    IOwnedPokemonRepository pokemonRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChooseStarterCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ChooseStarterCommand request, CancellationToken cancellationToken)
    {
        Trainer? trainer = await trainerRepository.GetAsync(request.TrainerId, cancellationToken);

        if (trainer is null)
        {
            return Result.Failure<Guid>(TrainerErrors.NotFound(request.TrainerId));
        }

        if (!await speciesRepository.ExistsAsync(request.SpeciesId, cancellationToken))
        {
            return Result.Failure<Guid>(SpeciesErrors.NotFound(request.SpeciesId));
        }

        Result choice = trainer.ChooseStarter(request.SpeciesId);

        if (choice.IsFailure)
        {
            return Result.Failure<Guid>(choice.Error);
        }

        Result<OwnedPokemon> starter = OwnedPokemon.Obtain(
            trainer.Id,
            request.SpeciesId,
            OwnedPokemon.StarterLevel,
            isShiny: false,
            PokemonOrigin.Starter,
            dateTimeProvider.UtcNow);

        if (starter.IsFailure)
        {
            return Result.Failure<Guid>(starter.Error);
        }

        pokemonRepository.Insert(starter.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return starter.Value.Id;
    }
}
