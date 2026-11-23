namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// A main-series game's Pokédex.
/// </summary>
/// <param name="Slug">How the Pokédex filter names the game.</param>
/// <param name="VersionGroup">PokéAPI's name for the game, whose Pokédexes list its Pokémon.</param>
/// <param name="NationalMaxSpeciesId">
/// Where the game's National Pokédex ends; empty when the game doesn't have one.
/// </param>
public sealed record GameDex(string Slug, string Name, int Generation, string VersionGroup, int? NationalMaxSpeciesId);

public static class GameDexes
{
    public static readonly IReadOnlyList<GameDex> All =
    [
        new("red-blue", "Red & Blue", 1, "red-blue", null),
        new("yellow", "Yellow", 1, "yellow", null),
        new("gold-silver", "Gold & Silver", 2, "gold-silver", null),
        new("crystal", "Crystal", 2, "crystal", null),
        new("ruby-sapphire", "Ruby & Sapphire", 3, "ruby-sapphire", 386),
        new("emerald", "Emerald", 3, "emerald", 386),
        new("firered-leafgreen", "FireRed & LeafGreen", 3, "firered-leafgreen", 386),
        new("diamond-pearl", "Diamond & Pearl", 4, "diamond-pearl", 493),
        new("platinum", "Platinum", 4, "platinum", 493),
        new("heartgold-soulsilver", "HeartGold & SoulSilver", 4, "heartgold-soulsilver", 493),
        new("black-white", "Black & White", 5, "black-white", 649),
        new("black-2-white-2", "Black 2 & White 2", 5, "black-2-white-2", 649),
        new("x-y", "X & Y", 6, "x-y", 721),
        new("omega-ruby-alpha-sapphire", "Omega Ruby & Alpha Sapphire", 6, "omega-ruby-alpha-sapphire", 721),
        new("sun-moon", "Sun & Moon", 7, "sun-moon", 802),
        new("ultra-sun-ultra-moon", "Ultra Sun & Ultra Moon", 7, "ultra-sun-ultra-moon", 807),
        new("lets-go-pikachu-eevee", "Let's Go Pikachu & Eevee", 7, "lets-go-pikachu-lets-go-eevee", null),
        new("sword-shield", "Sword & Shield", 8, "sword-shield", null),
        new("brilliant-diamond-shining-pearl", "Brilliant Diamond & Shining Pearl", 8, "brilliant-diamond-shining-pearl", 493),
        new("legends-arceus", "Legends: Arceus", 8, "legends-arceus", null),
        new("scarlet-violet", "Scarlet & Violet", 9, "scarlet-violet", null),
        new("legends-z-a", "Legends: Z-A", 9, "legends-za", null),
    ];

    public static GameDex? Find(string slug) => All.FirstOrDefault(g => g.Slug == slug);
}
