using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.Games;

/// <summary>
/// A game connected to PokeHub. Each game has its own Keycloak client: players sign in from
/// the game with it, and the client id in their token tells PokeHub which game is calling.
/// </summary>
public sealed class Game : Entity
{
    private Game()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    // Short, url-friendly name; also builds the client id: game-{slug}.
    public string Slug { get; private set; }

    // Where players open the game.
    public Uri Url { get; private set; }

    public string ClientId { get; private set; }

    // Origins the game runs on, allowed to call the API from the browser (CORS).
    public string[] WebOrigins { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime RegisteredOnUtc { get; private set; }

    public static string ClientIdFor(string slug) => $"game-{slug}";

    public static Game Register(string name, string slug, Uri url, string[] webOrigins, DateTime utcNow)
    {
        var game = new Game
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Slug = slug,
            Url = url,
            ClientId = ClientIdFor(slug),
            WebOrigins = webOrigins,
            IsActive = true,
            RegisteredOnUtc = utcNow,
        };

        game.Raise(new GameRegisteredDomainEvent(game.Id, game.ClientId));

        return game;
    }
}
