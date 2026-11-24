using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.PublicApi;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Application.Saves.GameSpecies;
using PokeHub.Modules.Pokedex.Application.Saves.GetOrCreateSave;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.CatchPokemon;

internal sealed class CatchPokemonCommandHandler(
    IOwnedPokemonRepository pokemonRepository,
    ISpeciesRepository speciesRepository,
    ITrainerRepository trainerRepository,
    ISaveRepository saveRepository,
    IGameSpeciesLimits speciesLimits,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CatchPokemonCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CatchPokemonCommand request, CancellationToken cancellationToken)
    {
        // A repeated request (the game retried after a timeout) gets the Pokémon it already created.
        // The key may also belong to a Pokémon the trainer received in a trade and delivered here.
        OwnedPokemon? existing = await pokemonRepository.GetByCatchKeyAsync(request.GameId, request.CatchKey, cancellationToken);

        if (existing is not null)
        {
            if (existing.TrainerId != request.TrainerId)
            {
                return Result.Failure<Guid>(PokemonErrors.CatchKeyInUse(request.CatchKey));
            }

            Result evolved = await RecordEvolutionAsync(existing, request, cancellationToken);

            return evolved.IsFailure ? Result.Failure<Guid>(evolved.Error) : existing.Id;
        }

        // Caught in another game and never put in this one by PokeHub: a save copied from that
        // game. Recording it would pay its catches again, in a game the player never played.
        if (request.FromOtherGame)
        {
            return Result.Failure<Guid>(PokemonErrors.FromOtherGame);
        }

        if (!await speciesRepository.ExistsAsync(request.SpeciesId, cancellationToken))
        {
            return Result.Failure<Guid>(SpeciesErrors.NotFound(request.SpeciesId));
        }

        Result fits = await GameSpeciesRule.CheckAsync(
            speciesLimits,
            request.GameId,
            request.GameName,
            request.SpeciesId,
            cancellationToken);

        if (fits.IsFailure)
        {
            return Result.Failure<Guid>(fits.Error);
        }

        Result<Save> save = await GetOrCreateSaveCommandHandler.GetOrCreateAsync(
            trainerRepository,
            saveRepository,
            dateTimeProvider,
            request.TrainerId,
            request.GameId,
            request.GameName,
            cancellationToken);

        if (save.IsFailure)
        {
            return Result.Failure<Guid>(save.Error);
        }

        Result<OwnedPokemon> pokemon = OwnedPokemon.Catch(
            save.Value,
            request.SpeciesId,
            request.Level,
            request.IsShiny,
            request.CatchKey,
            dateTimeProvider.UtcNow,
            request.Nickname);

        if (pokemon.IsFailure)
        {
            return Result.Failure<Guid>(pokemon.Error);
        }

        pokemonRepository.Insert(pokemon.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pokemon.Value.Id;
    }

    // A Pokémon the game already reported, now at a later form. An earlier form means an older
    // save was restored, which isn't an evolution and changes nothing.
    private async Task<Result> RecordEvolutionAsync(
        OwnedPokemon pokemon,
        CatchPokemonCommand request,
        CancellationToken cancellationToken)
    {
        if (pokemon.SpeciesId == request.SpeciesId)
        {
            return Result.Success();
        }

        Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId, cancellationToken);

        if (save is null || pokemon.SaveId != save.Id || !pokemon.IsInsideGame)
        {
            return Result.Success();
        }

        IReadOnlyList<int> stages = await speciesRepository.GetEvolutionPathAsync(
            pokemon.SpeciesId,
            request.SpeciesId,
            cancellationToken);

        foreach (int stage in stages)
        {
            Result result = pokemon.EvolveInGame(request.TrainerId, save, stage, dateTimeProvider.UtcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        if (stages.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
