using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Application.Saves.CancelWithdrawal;

internal sealed class CancelWithdrawalCommandHandler(
    ISaveRepository saveRepository,
    IOwnedPokemonRepository pokemonRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelWithdrawalCommand>
{
    public async Task<Result> Handle(CancelWithdrawalCommand request, CancellationToken cancellationToken)
    {
        Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId, cancellationToken);

        if (save is null)
        {
            return Result.Failure(PokemonErrors.NotInThisSave);
        }

        // Only a Pokémon the save still holds can go back; otherwise it would exist nowhere.
        var present = request.PresentCatchKeys.ToHashSet(StringComparer.Ordinal);
        DateTime utcNow = dateTimeProvider.UtcNow;

        foreach (string catchKey in request.CatchKeys.Distinct(StringComparer.Ordinal))
        {
            OwnedPokemon? pokemon = present.Contains(catchKey)
                ? await pokemonRepository.GetByCatchKeyAsync(save.GameId, catchKey, cancellationToken)
                : null;

            if (pokemon is null)
            {
                return Result.Failure(PokemonErrors.NotInThisSave);
            }

            Result result = pokemon.CancelWithdrawal(request.TrainerId, save, utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
