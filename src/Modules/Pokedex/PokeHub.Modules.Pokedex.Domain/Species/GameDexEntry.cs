namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// A species listed in a game's own Pokédex, at its place in that game's order.
/// </summary>
public sealed class GameDexEntry
{
    private GameDexEntry()
    {
    }

    public string GameSlug { get; private set; }

    public int SpeciesId { get; private set; }

    public int Position { get; private set; }

    public static GameDexEntry Create(string gameSlug, int speciesId, int position)
    {
        return new GameDexEntry { GameSlug = gameSlug, SpeciesId = speciesId, Position = position };
    }
}
