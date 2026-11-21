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
    /// The game that knows this Pokémon by <see cref="CatchKey"/>: the game it was caught in,
    /// or the last game it was delivered into.
    /// </summary>
    public Guid? CatchKeyGameId { get; private set; }

    /// <summary>
    /// The game's own identifier for the Pokémon. A game that repeats a request (after a
    /// timeout, say) sends the same key and gets the same Pokémon back instead of a second one.
    /// Kept when the Pokémon leaves the game, so it is recognized if it comes back.
    /// </summary>
    public string? CatchKey { get; private set; }

    /// <summary>
    /// Moved into a save on PokeHub, but not yet written into the game's own save. The game
    /// delivers it (<see cref="Deliver"/>) the next time the player opens it.
    /// </summary>
    public bool AwaitingDelivery { get; private set; }

    /// <summary>
    /// The Pokémon exactly as the last game that held it stored it (moves, stats, ...), in
    /// <see cref="GameDataFormat"/> (pk3 for Generation III). A game that reads the format
    /// writes it back unchanged when the Pokémon returns.
    /// </summary>
    public string? GameDataFormat { get; private set; }

    public byte[]? GameData { get; private set; }

    /// <summary>
    /// In a save and in the game's own save too: only the game can take it out.
    /// </summary>
    public bool IsInsideGame => SaveId is not null && !AwaitingDelivery && CatchKey is not null;

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

    /// <summary>
    /// A Pokémon bought in the shop, in the hub. Its id is the purchase's, so buying is safe
    /// to repeat: the same purchase can only ever create one Pokémon.
    /// </summary>
    public static Result<OwnedPokemon> Buy(Guid purchaseId, Guid trainerId, int speciesId, int level, bool isShiny, DateTime utcNow)
    {
        return Obtain(trainerId, speciesId, level, isShiny, PokemonOrigin.Shop, utcNow, saveId: null, id: purchaseId);
    }

    private static Result<OwnedPokemon> Obtain(
        Guid trainerId,
        int speciesId,
        int level,
        bool isShiny,
        PokemonOrigin origin,
        DateTime utcNow,
        Guid? saveId,
        Guid? id = null)
    {
        if (level is < MinLevel or > MaxLevel)
        {
            return Result.Failure<OwnedPokemon>(PokemonErrors.InvalidLevel(level));
        }

        var pokemon = new OwnedPokemon
        {
            Id = id ?? Guid.CreateVersion7(),
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

        pokemon.Raise(new PokemonObtainedDomainEvent(pokemon.Id, trainerId, speciesId, origin, isShiny));

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

        caught.Value.CatchKeyGameId = save.GameId;
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

        if (fromSaveId is not null)
        {
            RaiseSaveChanged(fromSaveId.Value, SaveChange.Released);
        }

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

            Raise(new PokemonEvolvedDomainEvent(Id, newTrainerId, fromSpeciesId, SpeciesId));
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

        RaiseSaveChanged(saveId, SaveChange.LeftGame);

        Raise(new PokemonReleasedDomainEvent(Id, trainerId, SpeciesId));

        return Result.Success();
    }

    /// <summary>
    /// Moves the Pokémon into one of the trainer's saves, or back to the hub when
    /// <paramref name="save"/> is empty. Moving between two games goes through the hub. A
    /// Pokémon moved into a save waits there until the game delivers it; one already inside a
    /// game can only be taken out by that game (<see cref="Withdraw"/>).
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

        if (IsInsideGame)
        {
            return Result.Failure(PokemonErrors.InsideGame);
        }

        Guid? previousSaveId = SaveId;

        SaveId = save?.Id;
        AwaitingDelivery = save is not null;

        Record(
            save is null ? OwnershipChange.MovedToHub : OwnershipChange.MovedToSave,
            trainerId,
            utcNow,
            saveId: save?.Id ?? previousSaveId);

        if (save is not null)
        {
            RaiseSaveChanged(save.Id, SaveChange.MovedIn);
        }
        else if (previousSaveId is not null)
        {
            RaiseSaveChanged(previousSaveId.Value, SaveChange.MovedOut);
        }

        return Result.Success();
    }

    /// <summary>
    /// The game wrote the Pokémon into its own save, where it knows it by
    /// <paramref name="catchKey"/>. Repeating the same delivery changes nothing.
    /// </summary>
    public Result Deliver(Guid trainerId, Save save, string catchKey)
    {
        Result ownership = EnsureActiveAndOwnedBy(trainerId);

        if (ownership.IsFailure)
        {
            return ownership;
        }

        if (SaveId != save.Id)
        {
            return Result.Failure(PokemonErrors.NotInThisSave);
        }

        if (!AwaitingDelivery)
        {
            return CatchKeyGameId == save.GameId && CatchKey == catchKey
                ? Result.Success()
                : Result.Failure(PokemonErrors.NotAwaitingDelivery);
        }

        CatchKeyGameId = save.GameId;
        CatchKey = catchKey;
        AwaitingDelivery = false;

        RaiseSaveChanged(save.Id, SaveChange.Delivered);

        return Result.Success();
    }

    /// <summary>
    /// The game took the Pokémon out of its own save and sent it to the hub, with what it
    /// stored about it: it may have grown, evolved or been renamed while in the game.
    /// </summary>
    public Result Withdraw(Guid trainerId, Save save, GameSnapshot snapshot, DateTime utcNow)
    {
        Result ownership = EnsureActiveAndOwnedBy(trainerId);

        if (ownership.IsFailure)
        {
            return ownership;
        }

        if (SaveId != save.Id || !IsInsideGame)
        {
            return Result.Failure(PokemonErrors.NotInThisSave);
        }

        if (snapshot.Level is < MinLevel or > MaxLevel)
        {
            return Result.Failure(PokemonErrors.InvalidLevel(snapshot.Level));
        }

        SaveId = null;
        Level = snapshot.Level;
        Nickname = string.IsNullOrWhiteSpace(snapshot.Nickname) ? null : snapshot.Nickname.Trim();
        GameDataFormat = snapshot.DataFormat;
        GameData = snapshot.Data;

        if (snapshot.SpeciesId != SpeciesId)
        {
            int fromSpeciesId = SpeciesId;
            SpeciesId = snapshot.SpeciesId;

            Record(OwnershipChange.EvolvedInGame, trainerId, utcNow, saveId: save.Id);

            Raise(new PokemonEvolvedDomainEvent(Id, trainerId, fromSpeciesId, SpeciesId));
        }

        Record(OwnershipChange.MovedToHub, trainerId, utcNow, saveId: save.Id);

        RaiseSaveChanged(save.Id, SaveChange.Withdrawn);

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

    private void RaiseSaveChanged(Guid saveId, SaveChange change) =>
        Raise(new PokemonSaveChangedDomainEvent(Id, TrainerId, SpeciesId, saveId, change));

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
