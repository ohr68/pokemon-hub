using MediatR;
using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Pokemon.CatchPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.ReleasePokemon;
using PokeHub.Modules.Pokedex.Application.Saves;
using PokeHub.Modules.Pokedex.Application.Saves.GetOrCreateSave;
using PokeHub.Modules.Pokedex.Application.Saves.ReconcileSave;
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
            pokemon.IsActive,
            pokemon.IsInHub);
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

    public async Task<Result<SaveInfo>> GetOrCreateSaveAsync(
        Guid trainerId,
        Guid gameId,
        string gameName,
        CancellationToken cancellationToken = default)
    {
        Result<SaveResponse> result = await sender.Send(
            new GetOrCreateSaveCommand(trainerId, gameId, gameName),
            cancellationToken);

        return result.IsFailure
            ? Result.Failure<SaveInfo>(result.Error)
            : new SaveInfo(result.Value.Id, result.Value.GameId, result.Value.GameName, result.Value.CreatedOnUtc);
    }

    public async Task<Result<IReadOnlyCollection<SavePokemonInfo>>> GetSavePokemonAsync(
        Guid trainerId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<PokemonResponse>> result = await sender.Send(
            new GetTrainerPokemonQuery(trainerId, GameId: gameId),
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<SavePokemonInfo>>(result.Error);
        }

        return result.Value
            .Select(p => new SavePokemonInfo(
                p.Id,
                p.SpeciesId,
                p.SpeciesName,
                p.Nickname,
                p.Level,
                p.IsShiny,
                p.IsOriginalTrainer))
            .ToList();
    }

    public Task<Result<Guid>> CatchPokemonAsync(CatchRequest request, CancellationToken cancellationToken = default)
    {
        return sender.Send(
            new CatchPokemonCommand(
                request.TrainerId,
                request.GameId,
                request.GameName,
                request.SpeciesId,
                request.Level,
                request.IsShiny,
                request.CatchKey,
                request.Nickname),
            cancellationToken);
    }

    public async Task<Result<IReadOnlyCollection<LeftGameInfo>>> ReconcileSaveAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<string> presentCatchKeys,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<LeftGameResponse>> result = await sender.Send(
            new ReconcileSaveCommand(trainerId, gameId, presentCatchKeys, dryRun),
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<LeftGameInfo>>(result.Error);
        }

        return result.Value
            .Select(p => new LeftGameInfo(p.Id, p.SpeciesId, p.Nickname, p.Level, p.IsShiny))
            .ToList();
    }

    public Task<Result> ReleaseFromSaveAsync(
        Guid trainerId,
        Guid gameId,
        Guid pokemonId,
        CancellationToken cancellationToken = default)
    {
        return sender.Send(new ReleasePokemonCommand(trainerId, pokemonId, gameId), cancellationToken);
    }
}
