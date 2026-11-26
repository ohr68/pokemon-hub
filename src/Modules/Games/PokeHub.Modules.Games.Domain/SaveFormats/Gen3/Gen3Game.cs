namespace PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

/// <summary>
/// A Gen 3 game whose save PokeHub edits, by slug: its save family, and where a Pokémon PokeHub
/// creates for it comes from, as if the game had caught it. Mirrors pokemon-web's
/// <c>src/lib/gen3/save-games.ts</c>; the 3DS ports write their original game's numbers.
/// </summary>
/// <param name="OriginGame">The number the game writes as a Pokémon's origin when it catches one.</param>
/// <param name="MetLocation">A town of its region, in the game's own numbering.</param>
public sealed record Gen3Game(Gen3Family Family, int OriginGame, int MetLocation)
{
    private const int PalletTown = 0x58;
    private const int LittlerootTown = 0x00;

    private static readonly Dictionary<string, Gen3Game> Games = new(StringComparer.Ordinal)
    {
        ["firered"] = new(Gen3Family.FireRedLeafGreen, 4, PalletTown),
        ["leafgreen"] = new(Gen3Family.FireRedLeafGreen, 5, PalletTown),
        ["ruby"] = new(Gen3Family.RubySapphire, 2, LittlerootTown),
        ["sapphire"] = new(Gen3Family.RubySapphire, 1, LittlerootTown),
        ["emerald"] = new(Gen3Family.Emerald, 3, LittlerootTown),
        ["twin-embers"] = new(Gen3Family.FireRedLeafGreen, 4, PalletTown),
        ["emerald-3ds"] = new(Gen3Family.Emerald, 3, LittlerootTown),
    };

    /// <summary>
    /// The game behind a slug, or null when PokeHub doesn't edit its saves. The admin form
    /// suggests slugs from the name ("Pokémon Ruby" becomes pokemon-ruby), so a leading
    /// "pokemon-" names the same game.
    /// </summary>
    public static Gen3Game? ForSlug(string slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        const string prefix = "pokemon-";
        string name = slug.StartsWith(prefix, StringComparison.Ordinal) ? slug[prefix.Length..] : slug;
        return Games.GetValueOrDefault(name);
    }
}
