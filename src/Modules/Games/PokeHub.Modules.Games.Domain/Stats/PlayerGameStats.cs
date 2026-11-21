using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.Stats;

/// <summary>
/// A player's numbers in one game, as the game last reported them. Each game sends the counters
/// it has, so they are kept by name.
/// </summary>
public sealed class PlayerGameStats : Entity
{
    private PlayerGameStats()
    {
    }

    public Guid TrainerId { get; private set; }

    public Guid GameId { get; private set; }

    public long PlayTimeSeconds { get; private set; }

    public long Money { get; private set; }

    public int PokedexSeen { get; private set; }

    public int PokedexOwned { get; private set; }

    // Counter name to value, as JSON.
    public string Counters { get; private set; }

    public DateTime UpdatedOnUtc { get; private set; }

    public static PlayerGameStats Create(Guid trainerId, Guid gameId) => new()
    {
        TrainerId = trainerId,
        GameId = gameId,
        Counters = "{}",
    };

    public void Report(long playTimeSeconds, long money, int pokedexSeen, int pokedexOwned, string counters, DateTime utcNow)
    {
        PlayTimeSeconds = playTimeSeconds;
        Money = money;
        PokedexSeen = pokedexSeen;
        PokedexOwned = pokedexOwned;
        Counters = counters;
        UpdatedOnUtc = utcNow;
    }
}
