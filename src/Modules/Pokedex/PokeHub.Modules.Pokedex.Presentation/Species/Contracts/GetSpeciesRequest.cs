namespace PokeHub.Modules.Pokedex.Presentation.Species.Contracts;

// Bound from the query string: ?search=char&type=fire&generation=1&page=1&pageSize=50
internal sealed class GetSpeciesRequest
{
    public string? Search { get; init; }

    public string? Type { get; init; }

    public int? Generation { get; init; }

    // A game's Pokédex (?game=firered-leafgreen), or its National one (&national=true).
    public string? Game { get; init; }

    public bool National { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 50;
}
