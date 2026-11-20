using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.Play;

internal static class PlayingGame
{
    /// <summary>
    /// The registered, active game behind the calling client. Requests from other clients
    /// (pokemon-web, a game that was turned off) are refused.
    /// </summary>
    internal static async Task<Result<Game>> ResolveAsync(
        IGameRepository gameRepository,
        string? clientId,
        CancellationToken cancellationToken)
    {
        Game? game = string.IsNullOrEmpty(clientId)
            ? null
            : await gameRepository.GetByClientIdAsync(clientId, cancellationToken);

        return game is { IsActive: true } ? game : Result.Failure<Game>(GameErrors.NotAGame);
    }
}
