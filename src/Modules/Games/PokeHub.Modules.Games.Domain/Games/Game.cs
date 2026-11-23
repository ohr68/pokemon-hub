using System.Security.Cryptography;
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

    // Requests one player may make to the game API per minute, unless the game sets its own.
    public const int DefaultRateLimitPerMinute = 600;
    public const int MinRateLimitPerMinute = 10;
    public const int MaxRateLimitPerMinute = 10_000;

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    // Shown to trainers in the games list.
    public string? Description { get; private set; }

    // Short, url-friendly name; also builds the client id: game-{slug}.
    public string Slug { get; private set; }

    // Where players open the game.
    public Uri Url { get; private set; }

    public string ClientId { get; private set; }

    // Origins the game runs on, allowed to call the API from the browser (CORS).
    public string[] WebOrigins { get; private set; }

    // A game turned off disappears from the list, and its players can't sign in or call the API.
    public bool IsActive { get; private set; }

    public int RateLimitPerMinute { get; private set; }

    /// <summary>
    /// The last National Dex number that exists in this game; empty when it takes any Pokémon.
    /// </summary>
    public int? MaxSpeciesId { get; private set; }

    /// <summary>
    /// Where PokeHub sends the game's events (a Pokémon moved into or out of its save), if
    /// anywhere. Each request is signed with <see cref="WebhookSecret"/>.
    /// </summary>
    public Uri? WebhookUrl { get; private set; }

    public string? WebhookSecret { get; private set; }

    public DateTime RegisteredOnUtc { get; private set; }

    public static string ClientIdFor(string slug) => $"game-{slug}";

    public static Game Register(
        string name,
        string slug,
        string? description,
        Uri url,
        string[] webOrigins,
        DateTime utcNow)
    {
        var game = new Game
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            RateLimitPerMinute = DefaultRateLimitPerMinute,
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

    public void UpdateDetails(string name, string? description, int rateLimitPerMinute, int? maxSpeciesId)
    {
        Name = name;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        RateLimitPerMinute = rateLimitPerMinute;
        MaxSpeciesId = maxSpeciesId;
    }

    public void TurnOn()
    {
        IsActive = true;
    }

    public void TurnOff()
    {
        IsActive = false;
    }

    /// <summary>
    /// Sends the game's events to <paramref name="url"/>, or stops sending them when empty. A
    /// new secret is created the first time.
    /// </summary>
    public void SetWebhook(Uri? url)
    {
        WebhookUrl = url;
        WebhookSecret = url is null ? null : WebhookSecret ?? NewSecret();
    }

    public Result RotateWebhookSecret()
    {
        if (WebhookUrl is null)
        {
            return Result.Failure(GameErrors.NoWebhook);
        }

        WebhookSecret = NewSecret();

        return Result.Success();
    }

    private static string NewSecret() => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
}
