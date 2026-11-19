using PokeHub.Common.Domain.Abstractions;

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

    public IReadOnlyCollection<OwnershipRecord> History => _history.AsReadOnly();

    public static Result<OwnedPokemon> Obtain(
        Guid trainerId,
        int speciesId,
        int level,
        bool isShiny,
        PokemonOrigin origin,
        DateTime utcNow)
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
        };

        pokemon.Record(OwnershipChange.Obtained, trainerId, utcNow);

        pokemon.Raise(new PokemonObtainedDomainEvent(pokemon.Id, trainerId, speciesId, origin));

        return pokemon;
    }

    public Result Release(Guid trainerId, DateTime utcNow)
    {
        Result ownership = EnsureActiveAndOwnedBy(trainerId);

        if (ownership.IsFailure)
        {
            return ownership;
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

    private void Record(OwnershipChange change, Guid trainerId, DateTime utcNow, Guid? tradeId = null)
    {
        _history.Add(OwnershipRecord.Create(Id, trainerId, SpeciesId, change, utcNow, tradeId));
    }
}
