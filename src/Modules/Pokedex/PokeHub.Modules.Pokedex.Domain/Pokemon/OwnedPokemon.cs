using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

/// <summary>
/// One individual Pokémon and its current owner. Every change of ownership is recorded in
/// <see cref="History"/> in the same transaction as the change, so counts and Pokédex
/// progress calculated from the history always match the actual state.
/// </summary>
public sealed class OwnedPokemon : Entity
{
    public const int MinLevel = 1;
    public const int MaxLevel = 100;
    public const int StarterLevel = 5;

    private readonly List<OwnershipRecord> _history = [];

    private OwnedPokemon()
    {
    }

    public Guid Id { get; private set; }

    public int SpeciesId { get; private set; }

    public Guid TrainerId { get; private set; }

    public Guid OriginalTrainerId { get; private set; }

    public string? Nickname { get; private set; }

    public int Level { get; private set; }

    public bool IsShiny { get; private set; }

    public PokemonOrigin Origin { get; private set; }

    public PokemonStatus Status { get; private set; }

    public DateTime ObtainedOnUtc { get; private set; }

    /// <summary>
    /// Where the Pokémon is now: in a game's save, or in the PokeHub hub when empty.
    /// </summary>
    public Guid? SaveId { get; private set; }

    /// <summary>
    /// The game it was caught in, for Pokémon caught in a game.
    /// </summary>
    public Guid? CaughtInGameId { get; private set; }

    /// <summary>
    /// The game's own identifier for the catch. A game that repeats a request (after a
    /// timeout, say) sends the same key and gets the same Pokémon back instead of a second one.
    /// </summary>
    public string? CatchKey { get; private set; }

    public bool IsInHub => SaveId is null;

    public IReadOnlyCollection<OwnershipRecord> History => _history.AsReadOnly();

    /// <summary>
    /// A Pokémon a trainer gets in the hub (a starter, or one given by an admin).
    /// </summary>
    public static Result<OwnedPokemon> Obtain(
        Guid trainerId,
        int speciesId,
        int level,
        bool isShiny,
        PokemonOrigin origin,
        DateTime utcNow)
    {
        return Obtain(trainerId, speciesId, level, isShiny, origin, utcNow, saveId: null);
    }

    private static Result<OwnedPokemon> Obtain(
        Guid trainerId,
        int speciesId,
        int level,
        bool isShiny,
        PokemonOrigin origin,
        DateTime utcNow,
        Guid? saveId)
    {
        if (level is < MinLevel or > MaxLevel)
        {
            return Result.Failure<OwnedPokemon>(PokemonErrors.InvalidLevel(level));
        }

        var pokemon = new OwnedPokemon
        {
            Id = Guid.CreateVersion7(),
            SpeciesId = speciesId,
            TrainerId = trainerId,
            OriginalTrainerId = trainerId,
            Level = level,
            IsShiny = isShiny,
            Origin = origin,
            Status = PokemonStatus.Active,
            ObtainedOnUtc = utcNow,
            SaveId = saveId,
        };

        pokemon.Record(OwnershipChange.Obtained, trainerId, utcNow, saveId: saveId);

        pokemon.Raise(new PokemonObtainedDomainEvent(pokemon.Id, trainerId, speciesId, origin));

        return pokemon;
    }

    /// <summary>
    /// A Pokémon caught in a game. It starts in that game's save.
    /// </summary>
    public static Result<OwnedPokemon> Catch(
        Save save,
        int speciesId,
        int level,
        bool isShiny,
        string catchKey,
        DateTime utcNow,
        string? nickname = null)
    {
        Result<OwnedPokemon> caught = Obtain(
            save.TrainerId,
            speciesId,
            level,
            isShiny,
            PokemonOrigin.Game,
            utcNow,
            save.Id);

        if (caught.IsFailure)
        {
            return caught;
        }

        caught.Value.CaughtInGameId = save.GameId;
        caught.Value.CatchKey = catchKey;
        caught.Value.Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();

        return caught;
    }

    /// <summary>
    /// Releases the Pokémon from where it is: the hub (<paramref name="fromSaveId"/> empty) or
    /// the given save, so a game can only release Pokémon in its own save.
    /// </summary>
    public Result Release(Guid trainerId, DateTime utcNow, Guid? fromSaveId = null)
    {
        Result ownership = EnsureActiveAndOwnedBy(trainerId);

        if (ownership.IsFailure)
        {
            return ownership;
        }

        if (SaveId != fromSaveId)
        {
            return Result.Failure(fromSaveId is null ? PokemonErrors.NotInHub : PokemonErrors.NotInThisSave);
        }

        Status = PokemonStatus.Released;

        Record(OwnershipChange.Released, trainerId, utcNow);

        Raise(new PokemonReleasedDomainEvent(Id, trainerId, SpeciesId));

        return Result.Success();
    }

    /// <summary>
    /// Moves the Pokémon to another trainer as part of a trade. The original trainer is kept,
    /// like in the games, and the Pokémon evolves when its species evolves by trading.
    /// </summary>
    public Result TradeTo(
        Guid currentTrainerId,
        Guid newTrainerId,
        Guid tradeId,
        int? tradeEvolutionSpeciesId,
        DateTime utcNow)
    {
        Result ownership = EnsureActiveAndOwnedBy(currentTrainerId);

        if (ownership.IsFailure)
        {
            return ownership;
        }

        // Trades happen from the hub, like trading from Pokémon HOME.
        if (!IsInHub)
        {
            return Result.Failure(PokemonErrors.NotInHub);
        }

        int fromSpeciesId = SpeciesId;

        Record(OwnershipChange.TradedAway, currentTrainerId, utcNow, tradeId);

        TrainerId = newTrainerId;

        Record(OwnershipChange.ReceivedInTrade, newTrainerId, utcNow, tradeId);

        if (tradeEvolutionSpeciesId is not null)
        {
            SpeciesId = tradeEvolutionSpeciesId.Value;

            Record(OwnershipChange.EvolvedByTrade, newTrainerId, utcNow, tradeId);
        }

        Raise(new PokemonTradedDomainEvent(Id, tradeId, currentTrainerId, newTrainerId, fromSpeciesId, SpeciesId));

        return Result.Success();
    }

    /// <summary>
    /// The game reports the Pokémon is no longer in its save (released or traded away there).
    /// Only applies to a Pokémon that is in that game's save on PokeHub.
    /// </summary>
    public Result LeaveGame(Guid trainerId, Guid saveId, DateTime utcNow)
    {
        Result ownership = EnsureActiveAndOwnedBy(trainerId);

        if (ownership.IsFailure)
        {
            return ownership;
        }

        if (SaveId != saveId)
        {
            return Result.Failure(PokemonErrors.NotInThisSave);
        }

        Status = PokemonStatus.Released;

        Record(OwnershipChange.LeftGame, trainerId, utcNow, saveId: saveId);

        Raise(new PokemonReleasedDomainEvent(Id, trainerId, SpeciesId));

        return Result.Success();
    }

    /// <summary>
    /// Moves the Pokémon into one of the trainer's saves, or back to the hub when
    /// <paramref name="save"/> is empty. Moving between two games goes through the hub.
    /// </summary>
    public Result MoveTo(Guid trainerId, Save? save, DateTime utcNow)
    {
        Result ownership = EnsureActiveAndOwnedBy(trainerId);

        if (ownership.IsFailure)
        {
            return ownership;
        }

        if (save is not null && save.TrainerId != trainerId)
        {
            return Result.Failure(SaveErrors.NotFound(save.Id));
        }

        if (save?.Id == SaveId)
        {
            return Result.Failure(PokemonErrors.AlreadyThere);
        }

        if (save is not null && !IsInHub)
        {
            return Result.Failure(PokemonErrors.NotInHub);
        }

        Guid? previousSaveId = SaveId;

        SaveId = save?.Id;

        Record(
            save is null ? OwnershipChange.MovedToHub : OwnershipChange.MovedToSave,
            trainerId,
            utcNow,
            saveId: save?.Id ?? previousSaveId);

        return Result.Success();
    }

    public Result Rename(Guid trainerId, string? nickname)
    {
        Result ownership = EnsureActiveAndOwnedBy(trainerId);

        if (ownership.IsFailure)
        {
            return ownership;
        }

        Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim();

        return Result.Success();
    }

    private Result EnsureActiveAndOwnedBy(Guid trainerId)
    {
        if (TrainerId != trainerId)
        {
            return Result.Failure(PokemonErrors.NotOwnedByTrainer(Id));
        }

        return Status == PokemonStatus.Active
            ? Result.Success()
            : Result.Failure(PokemonErrors.NotActive(Id));
    }

    private void Record(
        OwnershipChange change,
        Guid trainerId,
        DateTime utcNow,
        Guid? tradeId = null,
        Guid? saveId = null)
    {
        _history.Add(OwnershipRecord.Create(Id, trainerId, SpeciesId, change, utcNow, tradeId, saveId));
    }
}
