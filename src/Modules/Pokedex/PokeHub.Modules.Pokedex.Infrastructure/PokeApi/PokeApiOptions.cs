namespace PokeHub.Modules.Pokedex.Infrastructure.PokeApi;

internal sealed class PokeApiOptions
{
    public Uri BaseUrl { get; init; } = new("https://pokeapi.co/api/v2/");
}
