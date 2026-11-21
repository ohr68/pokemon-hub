using MediatR;
using Microsoft.EntityFrameworkCore;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Pokemon.BuyPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.CatchPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.ReleasePokemon;
using PokeHub.Modules.Pokedex.Application.Saves;
using PokeHub.Modules.Pokedex.Application.Saves.GetOrCreateSave;
using PokeHub.Modules.Pokedex.Application.Saves.DeliverPokemon;
using PokeHub.Modules.Pokedex.Application.Saves.GetAwaitingDelivery;
using PokeHub.Modules.Pokedex.Application.Saves.ReconcileSave;
using PokeHub.Modules.Pokedex.Application.Saves.RedeliverPokemon;
using PokeHub.Modules.Pokedex.Application.Saves.UpdateBattleData;
using PokeHub.Modules.Pokedex.Application.Saves.WithdrawPokemon;
using PokeHub.Modules.Pokedex.Application.Pokemon.TradePokemon;
using PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Pokedex.Infrastructure.PublicApi;

internal sealed class PokedexApi(ISender sender, ISpeciesRepository speciesRepository) : IPokedexApi
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

    public Task<Result> UpdateBattleDataAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<PokemonBattleReport> reports,
        CancellationToken cancellationToken = default) =>
        sender.Send(
            new UpdateBattleDataCommand(
                trainerId,
                gameId,
                [.. reports.Select(r => new BattleReport(r.CatchKey, ToBattleData(r.Data)))]),
            cancellationToken);

    private static BattleData ToBattleData(PokemonBattleData data) => new(
        data.Nature,
        data.AbilitySlot,
        data.HeldItemId,
        data.Friendship,
        [.. data.Moves.Select(m => new BattleMove(m.Id, m.Pp, m.PpUps))],
        data.Ivs,
        data.Evs,
        data.Stats is null
            ? null
            : new BattleStats(
                data.Stats.CurrentHp,
                data.Stats.MaxHp,
                data.Stats.Attack,
                data.Stats.Defense,
                data.Stats.Speed,
                data.Stats.SpecialAttack,
                data.Stats.SpecialDefense));

    public Task<Result> RedeliverAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<Guid> pokemonIds,
        IReadOnlyCollection<string> presentCatchKeys,
        CancellationToken cancellationToken = default) =>
        sender.Send(new RedeliverPokemonCommand(trainerId, gameId, pokemonIds, presentCatchKeys), cancellationToken);

    public Task<Result> ReleaseFromSaveAsync(
        Guid trainerId,
        Guid gameId,
        Guid pokemonId,
        CancellationToken cancellationToken = default)
    {
        return sender.Send(new ReleasePokemonCommand(trainerId, pokemonId, gameId), cancellationToken);
    }

    public async Task<Result<IReadOnlyCollection<DeliveryInfo>>> GetAwaitingDeliveryAsync(
        Guid trainerId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyCollection<AwaitingDeliveryResponse>> result = await sender.Send(
            new GetAwaitingDeliveryQuery(trainerId, gameId),
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<DeliveryInfo>>(result.Error);
        }

        return result.Value
            .Select(p => new DeliveryInfo(
                p.Id,
                p.SpeciesId,
                p.SpeciesName,
                p.Nickname,
                p.Level,
                p.IsShiny,
                p.IsOriginalTrainer,
                p.OriginalTrainerId,
                p.OriginalTrainerName,
                p.GameDataFormat,
                p.GameData))
            .ToList();
    }

    public async Task<Result> DeliverPokemonAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<DeliveredPokemon> deliveries,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await sender.Send(
                new DeliverPokemonCommand(
                    trainerId,
                    gameId,
                    deliveries.Select(d => new Delivery(d.PokemonId, d.CatchKey)).ToList()),
                cancellationToken);
        }
        catch (PokeHubException exception) when (exception.InnerException is DbUpdateConcurrencyException)
        {
            return Result.Failure(PokemonErrors.ChangedMeanwhile);
        }
    }

    public async Task<Result<IReadOnlyCollection<Guid>>> WithdrawPokemonAsync(
        Guid trainerId,
        Guid gameId,
        IReadOnlyCollection<WithdrawnPokemon> withdrawals,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await sender.Send(
                new WithdrawPokemonCommand(
                    trainerId,
                    gameId,
                    withdrawals
                        .Select(w => new Withdrawal(w.CatchKey, w.SpeciesId, w.Level, w.Nickname, w.DataFormat, w.Data))
                        .ToList()),
                cancellationToken);
        }
        catch (PokeHubException exception) when (exception.InnerException is DbUpdateConcurrencyException)
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(PokemonErrors.ChangedMeanwhile);
        }
    }

    public Task<Result<Guid>> GiveBoughtPokemonAsync(
        Guid purchaseId,
        Guid trainerId,
        int speciesId,
        int level,
        bool isShiny,
        CancellationToken cancellationToken = default)
    {
        return sender.Send(new GiveBoughtPokemonCommand(purchaseId, trainerId, speciesId, level, isShiny), cancellationToken);
    }

    public async Task<SpeciesInfo?> GetSpeciesAsync(int speciesId, CancellationToken cancellationToken = default)
    {
        PokemonSpecies? species = await speciesRepository.GetAsync(speciesId, cancellationToken);

        return species is null ? null : new SpeciesInfo(species.Id, species.Name, species.IsRare);
    }
}
