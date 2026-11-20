namespace PokeHub.Modules.Games.PublicApi;

/// <summary>
/// Lets the API host's CORS policy accept browser requests from registered games.
/// </summary>
public interface IGameOriginPolicy
{
    bool IsAllowed(string origin);
}
