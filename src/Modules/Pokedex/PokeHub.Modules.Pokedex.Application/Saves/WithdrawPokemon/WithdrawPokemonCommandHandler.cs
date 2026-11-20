using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Saves.WithdrawPokemon;

internal sealed class WithdrawPokemonCommandHandler(
    ISaveRepository saveRepository,
    IOwnedPokemonRepository pokemonRepository,
    ISpeciesRepository speciesRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<WithdrawPokemonCommand, IReadOnlyCollection<Guid>>
{
    public async Task<Result<IReadOnlyCollection<Guid>>> Handle(
        WithdrawPokemonCommand request,
        CancellationToken cancellationToken)
    {
        Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId, cancellationToken);

        if (save is null)
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(PokemonErrors.NotInThisSave);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        var withdrawn = new List<Guid>(request.Withdrawals.Count);

        foreach (Withdrawal withdrawal in request.Withdrawals)
        {
            OwnedPokemon? pokemon = await pokemonRepository.GetByCatchKeyAsync(save.GameId, withdrawal.CatchKey, cancellationToken);

            if (pokemon is null)
            {
                return Result.Failure<IReadOnlyCollection<Guid>>(PokemonErrors.NotInThisSave);
            }

            if (!await speciesRepository.ExistsAsync(withdrawal.SpeciesId, cancellationToken))
            {
                return Result.Failure<IReadOnlyCollection<Guid>>(SpeciesErrors.NotFound(withdrawal.SpeciesId));
            }

            Result result = pokemon.Withdraw(
                request.TrainerId,
                save,
                new GameSnapshot(withdrawal.SpeciesId, withdrawal.Level, withdrawal.Nickname, withdrawal.DataFormat, withdrawal.Data),
                utcNow);

            if (result.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<Guid>>(result.Error);
            }

            withdrawn.Add(pokemon.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return withdrawn;
    }
}
