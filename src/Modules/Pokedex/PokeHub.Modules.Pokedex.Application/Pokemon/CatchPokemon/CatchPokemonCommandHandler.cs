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
            return existing.TrainerId == request.TrainerId
                ? existing.Id
                : Result.Failure<Guid>(PokemonErrors.CatchKeyInUse(request.CatchKey));
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
}
