namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>A game the trainer reported badges from, and how many badges it has in all.</summary>
public sealed class TrainerGame
{
    private TrainerGame()
    {
    }

    public Guid TrainerId { get; private set; }

    public Guid GameId { get; private set; }

    public string GameName { get; private set; }

    public int TotalBadges { get; private set; }

    internal static TrainerGame Create(Guid trainerId, Guid gameId, string gameName, int totalBadges) => new()
    {
        TrainerId = trainerId,
        GameId = gameId,
        GameName = gameName,
        TotalBadges = totalBadges,
    };

    internal void Update(string gameName, int totalBadges)
    {
        GameName = gameName;
        TotalBadges = totalBadges;
    }
}

/// <summary>A gym badge the trainer earned in a game.</summary>
public sealed class TrainerBadge
{
    private TrainerBadge()
    {
    }

    public Guid TrainerId { get; private set; }

    public Guid GameId { get; private set; }

    // The game's own id for it.
    public string Code { get; private set; }

    public string Name { get; private set; }

    public DateTime EarnedOnUtc { get; private set; }

    internal static TrainerBadge Create(Guid trainerId, Guid gameId, string code, string name, DateTime earnedOnUtc) => new()
    {
        TrainerId = trainerId,
        GameId = gameId,
        Code = code,
        Name = name,
        EarnedOnUtc = earnedOnUtc,
    };
}
