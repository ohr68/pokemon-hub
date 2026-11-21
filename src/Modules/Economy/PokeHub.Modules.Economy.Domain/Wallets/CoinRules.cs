namespace PokeHub.Modules.Economy.Domain.Wallets;

/// <summary>What earns coins and how many: the one place to tune the economy.</summary>
public static class CoinRules
{
    public const int ShinyCatch = 50;

    // A legendary or mythical species.
    public const int RareCatch = 100;

    public const int Badge = 30;

    public static int Trophy(string tier) => tier switch
    {
        "gold" => 100,
        "silver" => 50,
        _ => 20,
    };

    // Reaching a level pays ten times that level: 20 for level 2, 100 for level 10.
    public static int Level(int level) => 10 * level;
}
