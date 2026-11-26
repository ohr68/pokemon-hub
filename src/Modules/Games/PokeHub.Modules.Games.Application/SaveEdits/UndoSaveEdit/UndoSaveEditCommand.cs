using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.PublicApi;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveEdits;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.SaveEdits.UndoSaveEdit;

/// <summary>
/// The device couldn't write an edited save into the game, which still has the save it sent:
/// everything the edit moved goes back right away, rather than on the next send. Delivered
/// Pokémon and shop items wait again; withdrawn Pokémon are back inside the game.
/// </summary>
/// <param name="ClientId">The client the player signed in with, from the token.</param>
public sealed record UndoSaveEditCommand(string? ClientId, Guid TrainerId, Guid EditId) : ICommand;

internal sealed class UndoSaveEditCommandHandler(
    IGameRepository gameRepository,
    ISaveEditRepository saveEditRepository,
    IPokedexApi pokedexApi,
    IEconomyApi economyApi,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UndoSaveEditCommand>
{
    public async Task<Result> Handle(UndoSaveEditCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);
        if (game.IsFailure)
        {
            return Result.Failure(game.Error);
        }

        SaveEdit? edit = await saveEditRepository.GetAsync(request.EditId, request.TrainerId, cancellationToken);
        if (edit is null || edit.GameId != game.Value.Id)
        {
            return Result.Failure(SaveEditErrors.NotFound);
        }

        if (edit.SettledOnUtc is not null)
        {
            return Result.Failure(SaveEditErrors.AlreadySettled);
        }

        // The game kept its old save: none of the delivered Pokémon are in it, and every
        // withdrawn one still is.
        if (edit.DeliveredPokemonIds.Count > 0)
        {
            Result redelivered = await pokedexApi.RedeliverAsync(
                request.TrainerId, edit.GameId, edit.DeliveredPokemonIds, [], cancellationToken);
            if (redelivered.IsFailure)
            {
                return redelivered;
            }
        }

        if (edit.WithdrawnCatchKeys.Count > 0)
        {
            Result cancelled = await pokedexApi.CancelWithdrawalAsync(
                request.TrainerId, edit.GameId, edit.WithdrawnCatchKeys, edit.WithdrawnCatchKeys, cancellationToken);
            if (cancelled.IsFailure)
            {
                return cancelled;
            }
        }

        if (edit.DeliveredItemIds.Count > 0)
        {
            Result returned = await economyApi.ReturnItemsAsync(request.TrainerId, edit.DeliveredItemIds, cancellationToken);
            if (returned.IsFailure)
            {
                return returned;
            }
        }

        edit.Settle(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
