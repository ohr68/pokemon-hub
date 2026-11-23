using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Trainers.SetAvatar;

internal sealed class SetAvatarCommandHandler(
    ITrainerRepository trainerRepository,
    IOwnedPokemonRepository pokemonRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetAvatarCommand>
{
    public async Task<Result> Handle(SetAvatarCommand request, CancellationToken cancellationToken)
    {
        Trainer? trainer = await trainerRepository.GetAsync(request.TrainerId, cancellationToken);

        if (trainer is null)
        {
            return Result.Failure(TrainerErrors.NotFound(request.TrainerId));
        }

        if (request.PokemonId is Guid pokemonId)
        {
            OwnedPokemon? pokemon = await pokemonRepository.GetAsync(pokemonId, cancellationToken);

            if (pokemon is null || pokemon.TrainerId != request.TrainerId || pokemon.Status != PokemonStatus.Active)
            {
                return Result.Failure(PokemonErrors.NotFound(pokemonId));
            }
        }

        trainer.SetAvatar(request.PokemonId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
