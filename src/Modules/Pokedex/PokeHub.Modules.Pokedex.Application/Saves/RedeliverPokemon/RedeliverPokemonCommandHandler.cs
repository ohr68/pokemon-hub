using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Application.Saves.RedeliverPokemon;

internal sealed class RedeliverPokemonCommandHandler(
    ISaveRepository saveRepository,
    IOwnedPokemonRepository pokemonRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RedeliverPokemonCommand>
{
    public async Task<Result> Handle(RedeliverPokemonCommand request, CancellationToken cancellationToken)
    {
        Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId, cancellationToken);

        if (save is null)
        {
            return Result.Failure(PokemonErrors.NotInThisSave);
        }

        var present = request.PresentCatchKeys.ToHashSet(StringComparer.Ordinal);

        var missing =
            (await pokemonRepository.GetInsideGameAsync(save.Id, request.GameId, cancellationToken))
            .Where(p => !present.Contains(p.CatchKey!))
            .ToDictionary(p => p.Id);

        foreach (Guid pokemonId in request.PokemonIds.Distinct())
        {
            if (!missing.TryGetValue(pokemonId, out OwnedPokemon? pokemon))
            {
                return Result.Failure(PokemonErrors.NotInThisSave);
            }

            Result result = pokemon.Redeliver(request.TrainerId, save);

            if (result.IsFailure)
            {
                return result;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
