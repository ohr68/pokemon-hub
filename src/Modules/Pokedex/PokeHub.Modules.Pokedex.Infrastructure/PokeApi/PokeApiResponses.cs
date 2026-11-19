using System.Text.Json.Serialization;

namespace PokeHub.Modules.Pokedex.Infrastructure.PokeApi;

// Only the fields the catalog sync reads.

internal sealed record NamedResource(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] Uri Url)
{
    // PokéAPI urls end in the resource id: .../pokemon-species/25/
    public int Id => int.Parse(Url.Segments[^1].TrimEnd('/'), System.Globalization.CultureInfo.InvariantCulture);
}

internal sealed record ResourceList(
    [property: JsonPropertyName("results")] IReadOnlyList<NamedResource> Results);

internal sealed record GenerationResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("pokemon_species")] IReadOnlyList<NamedResource> PokemonSpecies);

internal sealed record TypeResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("pokemon")] IReadOnlyList<TypeSlot> Pokemon);

internal sealed record TypeSlot(
    [property: JsonPropertyName("slot")] int Slot,
    [property: JsonPropertyName("pokemon")] NamedResource Pokemon);
