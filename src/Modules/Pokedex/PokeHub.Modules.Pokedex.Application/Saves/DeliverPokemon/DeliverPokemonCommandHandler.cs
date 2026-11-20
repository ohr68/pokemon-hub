using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Application.Saves.DeliverPokemon;

internal sealed class DeliverPokemonCommandHandler(
    ISaveRepository saveRepository,
    IOwnedPokemonRepository pokemonRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeliverPokemonCommand>
{
    public async Task<Result> Handle(DeliverPokemonCommand request, CancellationToken cancellationToken)
    {
        Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId, cancellationToken);

        if (save is null)
        {
            return Result.Failure(PokemonErrors.NotInThisSave);
        }

        foreach (Delivery delivery in request.Deliveries)
        {
            OwnedPokemon? pokemon = await pokemonRepository.GetAsync(delivery.PokemonId, cancellationToken);

            if (pokemon is null)
            {
                return Result.Failure(PokemonErrors.NotFound(delivery.PokemonId));
            }

            // The game must not know two Pokémon by the same key.
            OwnedPokemon? holder = await pokemonRepository.GetByCatchKeyAsync(save.GameId, delivery.CatchKey, cancellationToken);

            if (holder is not null && holder.Id != pokemon.Id)
            {
                return Result.Failure(PokemonErrors.CatchKeyInUse(delivery.CatchKey));
            }

            Result delivered = pokemon.Deliver(request.TrainerId, save, delivery.CatchKey);

            if (delivered.IsFailure)
            {
                return delivered;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
