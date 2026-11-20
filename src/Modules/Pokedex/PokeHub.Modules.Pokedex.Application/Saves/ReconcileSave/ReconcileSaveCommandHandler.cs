using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Application.Saves.ReconcileSave;

internal sealed class ReconcileSaveCommandHandler(
    ISaveRepository saveRepository,
    IOwnedPokemonRepository pokemonRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReconcileSaveCommand, IReadOnlyCollection<LeftGameResponse>>
{
    public async Task<Result<IReadOnlyCollection<LeftGameResponse>>> Handle(
        ReconcileSaveCommand request,
        CancellationToken cancellationToken)
    {
        Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId, cancellationToken);

        // No save yet means nothing was ever synced from this game: nothing to mirror.
        if (save is null)
        {
            return Array.Empty<LeftGameResponse>();
        }

        var present = request.PresentCatchKeys.ToHashSet(StringComparer.Ordinal);

        var left = (await pokemonRepository.GetInsideGameAsync(save.Id, request.GameId, cancellationToken))
            .Where(p => !present.Contains(p.CatchKey!))
            .ToList();

        if (request.DryRun || left.Count == 0)
        {
            return left.Select(ToResponse).ToList();
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        foreach (OwnedPokemon pokemon in left)
        {
            Result result = pokemon.LeaveGame(request.TrainerId, save.Id, utcNow);

            if (result.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<LeftGameResponse>>(result.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return left.Select(ToResponse).ToList();
    }

    private static LeftGameResponse ToResponse(OwnedPokemon pokemon) =>
        new(pokemon.Id, pokemon.SpeciesId, pokemon.Nickname, pokemon.Level, pokemon.IsShiny);
}
