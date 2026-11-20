namespace PokeHub.Modules.Games.PublicApi;

/// <summary>
/// The per-player request limit of each game's API, read by the host's rate limiter.
/// </summary>
public interface IGameRateLimits
{
    /// <summary>Requests one player may make per minute through the game with this client id.</summary>
    int GetLimitPerMinute(string clientId);
}

public static class GameRateLimiting
{
    // The host registers the policy under this name; the game API endpoints require it.
    public const string PolicyName = "game-api";
}
