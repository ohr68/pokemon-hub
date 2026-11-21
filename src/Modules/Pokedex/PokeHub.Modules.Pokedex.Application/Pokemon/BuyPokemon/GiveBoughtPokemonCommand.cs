using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.BuyPokemon;

/// <summary>
/// Gives a trainer the Pokémon they bought in the shop, in the hub. The Pokémon gets the
/// purchase's id, so repeating the same purchase returns the same Pokémon.
/// </summary>
public sealed record GiveBoughtPokemonCommand(Guid PurchaseId, Guid TrainerId, int SpeciesId, int Level, bool IsShiny) : ICommand<Guid>;

internal sealed class GiveBoughtPokemonCommandHandler(
    ITrainerRepository trainerRepository,
    ISpeciesRepository speciesRepository,
    IOwnedPokemonRepository pokemonRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<GiveBoughtPokemonCommand, Guid>
{
    public async Task<Result<Guid>> Handle(GiveBoughtPokemonCommand request, CancellationToken cancellationToken)
    {
        if (await pokemonRepository.GetAsync(request.PurchaseId, cancellationToken) is { } given)
        {
            return given.Id;
        }

        if (await trainerRepository.GetAsync(request.TrainerId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(TrainerErrors.NotFound(request.TrainerId));
        }

        if (!await speciesRepository.ExistsAsync(request.SpeciesId, cancellationToken))
        {
            return Result.Failure<Guid>(SpeciesErrors.NotFound(request.SpeciesId));
        }

        Result<OwnedPokemon> pokemon = OwnedPokemon.Buy(
            request.PurchaseId,
            request.TrainerId,
            request.SpeciesId,
            request.Level,
            request.IsShiny,
            dateTimeProvider.UtcNow);

        if (pokemon.IsFailure)
        {
            return Result.Failure<Guid>(pokemon.Error);
        }

        pokemonRepository.Insert(pokemon.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pokemon.Value.Id;
    }
}
