namespace PokeHub.Modules.Pokedex.PublicApi;

/// <param name="Name">PokéAPI's name, such as mr-mime.</param>
public sealed record SpeciesInfo(int Id, string Name, bool IsRare);
