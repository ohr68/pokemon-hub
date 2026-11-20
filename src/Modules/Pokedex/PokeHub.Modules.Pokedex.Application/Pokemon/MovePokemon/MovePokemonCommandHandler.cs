using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.MovePokemon;

internal sealed class MovePokemonCommandHandler(
    IOwnedPokemonRepository pokemonRepository,
    ISaveRepository saveRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<MovePokemonCommand>
{
    public async Task<Result> Handle(MovePokemonCommand request, CancellationToken cancellationToken)
    {
        OwnedPokemon? pokemon = await pokemonRepository.GetAsync(request.PokemonId, cancellationToken);

        if (pokemon is null || pokemon.TrainerId != request.TrainerId)
        {
            return Result.Failure(PokemonErrors.NotFound(request.PokemonId));
        }

        Save? save = null;

        if (request.SaveId is not null)
        {
            save = await saveRepository.GetAsync(request.SaveId.Value, cancellationToken);

            // Another trainer's save is reported as not found.
            if (save is null || save.TrainerId != request.TrainerId)
            {
                return Result.Failure(SaveErrors.NotFound(request.SaveId.Value));
            }
        }

        Result result = pokemon.MoveTo(request.TrainerId, save, dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
