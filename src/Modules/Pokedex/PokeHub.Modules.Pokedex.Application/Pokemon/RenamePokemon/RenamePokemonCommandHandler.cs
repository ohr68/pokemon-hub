using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.RenamePokemon;

internal sealed class RenamePokemonCommandHandler(IOwnedPokemonRepository pokemonRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RenamePokemonCommand>
{
    public async Task<Result> Handle(RenamePokemonCommand request, CancellationToken cancellationToken)
    {
        OwnedPokemon? pokemon = await pokemonRepository.GetAsync(request.PokemonId, cancellationToken);

        if (pokemon is null || pokemon.TrainerId != request.TrainerId)
        {
            return Result.Failure(PokemonErrors.NotFound(request.PokemonId));
        }

        Result result = pokemon.Rename(request.TrainerId, request.Nickname);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
