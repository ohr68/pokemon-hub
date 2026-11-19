using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GrantPokemon;

internal sealed class GrantPokemonCommandHandler(
    ITrainerRepository trainerRepository,
    ISpeciesRepository speciesRepository,
    IOwnedPokemonRepository pokemonRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GrantPokemonCommand, Guid>
{
    public async Task<Result<Guid>> Handle(GrantPokemonCommand request, CancellationToken cancellationToken)
    {
        if (await trainerRepository.GetAsync(request.TrainerId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(TrainerErrors.NotFound(request.TrainerId));
        }

        if (!await speciesRepository.ExistsAsync(request.SpeciesId, cancellationToken))
        {
            return Result.Failure<Guid>(SpeciesErrors.NotFound(request.SpeciesId));
        }

        Result<OwnedPokemon> pokemon = OwnedPokemon.Obtain(
            request.TrainerId,
            request.SpeciesId,
            request.Level,
            request.IsShiny,
            PokemonOrigin.Grant,
            dateTimeProvider.UtcNow);

        if (pokemon.IsFailure)
        {
            return Result.Failure<Guid>(pokemon.Error);
        }

        pokemonRepository.Insert(pokemon.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pokemon.Value.Id;
    }
}
