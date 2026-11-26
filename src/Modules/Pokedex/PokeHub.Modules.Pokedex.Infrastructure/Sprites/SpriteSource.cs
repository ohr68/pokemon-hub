using System.Net;
using PokeHub.Common.Application.Caching;
using PokeHub.Modules.Pokedex.Application.Sprites;

namespace PokeHub.Modules.Pokedex.Infrastructure.Sprites;

// Fetched once from PokéAPI's sprites on GitHub, then kept in the cache: they never change.
internal sealed class SpriteSource(HttpClient httpClient, ICacheService cacheService) : ISpriteSource
{
    public static readonly Uri BaseAddress = new("https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/");

    private static readonly TimeSpan Kept = TimeSpan.FromDays(30);

    public async Task<byte[]?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        string key = $"sprites:{path}";
        byte[]? cached = await cacheService.GetAsync<byte[]>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        using HttpResponseMessage response = await httpClient.GetAsync(new Uri(path, UriKind.Relative), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        byte[] png = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        await cacheService.SetAsync(key, png, Kept, cancellationToken);
        return png;
    }
}
