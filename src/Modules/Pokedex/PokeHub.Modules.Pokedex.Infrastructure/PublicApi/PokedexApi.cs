using MediatR;
using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.TradePokemon;
using PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Pokedex.Infrastructure.PublicApi;

internal sealed class PokedexApi(ISender sender) : IPokedexApi
{
    public async Task<PokemonInfo?> GetPokemonAsync(Guid pokemonId, CancellationToken cancellationToken = default)
    {
        Result<PokemonDetailsResponse> result = await sender.Send(new GetPokemonQuery(pokemonId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        PokemonDetailsResponse pokemon = result.Value;

        return new PokemonInfo(
            pokemon.Id,
            pokemon.TrainerId,
            pokemon.SpeciesId,
            pokemon.SpeciesName,
            pokemon.Nickname,
            pokemon.Level,
            pokemon.IsShiny,
            pokemon.IsActive);
    }

    public async Task<TrainerInfo?> GetTrainerAsync(Guid trainerId, CancellationToken cancellationToken = default)
    {
        Result<TrainerResponse> result = await sender.Send(new GetTrainerQuery(trainerId), cancellationToken);

        return result.IsFailure ? null : new TrainerInfo(result.Value.Id, result.Value.Name, result.Value.Code);
    }

    public async Task<Result> TradePokemonAsync(TradeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await sender.Send(
                new TradePokemonCommand(
                    request.TradeId,
                    request.FirstTrainerId,
                    request.FirstPokemonId,
                    request.SecondTrainerId,
                    request.SecondPokemonId),
                cancellationToken);
        }
        catch (PokeHubException exception) when (exception.InnerException is DbUpdateConcurrencyException)
        {
            // One of the Pokémon changed after it was read (another trade or a release won).
            return Result.Failure(PokemonErrors.TradeConflict);
        }
    }
}
