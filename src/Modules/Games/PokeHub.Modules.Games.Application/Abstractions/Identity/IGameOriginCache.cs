namespace PokeHub.Modules.Games.Application.Abstractions.Identity;

/// <summary>
/// The browser origins of registered games, kept in memory for CORS checks.
/// </summary>
public interface IGameOriginCache
{
    void Invalidate();
}
