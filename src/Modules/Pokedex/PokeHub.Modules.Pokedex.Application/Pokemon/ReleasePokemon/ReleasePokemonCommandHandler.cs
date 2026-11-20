using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.ReleasePokemon;

internal sealed class ReleasePokemonCommandHandler(
    IOwnedPokemonRepository pokemonRepository,
    ISaveRepository saveRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReleasePokemonCommand>
{
    public async Task<Result> Handle(ReleasePokemonCommand request, CancellationToken cancellationToken)
    {
        OwnedPokemon? pokemon = await pokemonRepository.GetAsync(request.PokemonId, cancellationToken);

        // Another trainer's Pokémon is reported as not found, so ids can't be probed.
        if (pokemon is null || pokemon.TrainerId != request.TrainerId)
        {
            return Result.Failure(PokemonErrors.NotFound(request.PokemonId));
        }

        Guid? fromSaveId = null;

        if (request.GameId is not null)
        {
            Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId.Value, cancellationToken);

            if (save is null)
            {
                return Result.Failure(PokemonErrors.NotInThisSave);
            }

            fromSaveId = save.Id;
        }

        Result result = pokemon.Release(request.TrainerId, dateTimeProvider.UtcNow, fromSaveId);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
