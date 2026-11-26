namespace PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

/// <summary>The three Gen 3 save families, which keep things in different places.</summary>
public enum Gen3Family
{
    FireRedLeafGreen,
    RubySapphire,
    Emerald,
}

/// <summary>A bag pocket PokeHub fills.</summary>
public enum Gen3Pocket
{
    Items,
    Balls,
}

/// <summary>Where a pocket starts in section 1, and how many slots it has.</summary>
public sealed record Gen3PocketLayout(int Offset, int Slots);

/// <summary>
/// Where each Gen 3 save family keeps what PokeHub reads and writes. The sections, the PC boxes
/// and the Pokémon themselves are the same in every Gen 3 game; the rest moved between
/// Ruby/Sapphire, FireRed/LeafGreen and Emerald. It mirrors pokemon-web's
/// <c>src/lib/gen3/layout.ts</c>, which real saves confirmed.
/// </summary>
public sealed record Gen3Layout(
    Gen3Family Family,
    // What the game writes at 0xAC in section 0; Emerald keeps its security key there instead.
    uint? GameCode,
    int TeamSizeOffset,
    int TeamOffset,
    // Section 0: the key item quantities are XORed with. Ruby/Sapphire store them as they are.
    int? SecurityKeyOffset,
    Gen3PocketLayout ItemsPocket,
    Gen3PocketLayout BallsPocket)
{
    public const int GameCodeOffset = 0xAC;

    public static readonly Gen3Layout FireRedLeafGreen = new(
        Gen3Family.FireRedLeafGreen,
        GameCode: 1,
        TeamSizeOffset: 0x34,
        TeamOffset: 0x38,
        SecurityKeyOffset: 0xF20,
        ItemsPocket: new Gen3PocketLayout(0x310, 42),
        BallsPocket: new Gen3PocketLayout(0x430, 13));

    public static readonly Gen3Layout RubySapphire = new(
        Gen3Family.RubySapphire,
        GameCode: 0,
        TeamSizeOffset: 0x234,
        TeamOffset: 0x238,
        SecurityKeyOffset: null,
        ItemsPocket: new Gen3PocketLayout(0x560, 20),
        BallsPocket: new Gen3PocketLayout(0x600, 16));

    public static readonly Gen3Layout Emerald = new(
        Gen3Family.Emerald,
        GameCode: null,
        TeamSizeOffset: 0x234,
        TeamOffset: 0x238,
        SecurityKeyOffset: GameCodeOffset,
        ItemsPocket: new Gen3PocketLayout(0x560, 30),
        BallsPocket: new Gen3PocketLayout(0x650, 16));

    public Gen3PocketLayout Pocket(Gen3Pocket pocket) => pocket == Gen3Pocket.Balls ? BallsPocket : ItemsPocket;

    /// <summary>
    /// The layout for a save, from the game code its section 0 holds: 1 is FireRed/LeafGreen, 0
    /// is Ruby/Sapphire and anything else is Emerald's key. A save for a game known to be
    /// Emerald is read as Emerald whatever it holds, since its key can happen to be 0 or 1.
    /// </summary>
    public static Gen3Layout For(uint gameCode, Gen3Family? family = null)
    {
        if (family == Gen3Family.Emerald)
        {
            return Emerald;
        }

        return gameCode switch
        {
            1 => FireRedLeafGreen,
            0 => RubySapphire,
            _ => Emerald,
        };
    }
}
