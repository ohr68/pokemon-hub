namespace PokeHub.Modules.Economy.Domain.Shop;

/// <summary>A bag item a shop can sell, by its id in the Generation III games.</summary>
public sealed record GameItem(int Id, string Name, string Pocket);

/// <summary>
/// The items that can be sold. Key items, TMs and event items are left out on purpose: they can
/// break the story or the game's checks.
/// </summary>
public static class GameItems
{
    public const string Items = "items";
    public const string Balls = "balls";

    public static readonly IReadOnlyList<GameItem> All =
    [
        new(1, "Master Ball", Balls),
        new(2, "Ultra Ball", Balls),
        new(3, "Great Ball", Balls),
        new(4, "Poké Ball", Balls),
        new(13, "Potion", Items),
        new(19, "Full Restore", Items),
        new(20, "Max Potion", Items),
        new(21, "Hyper Potion", Items),
        new(22, "Super Potion", Items),
        new(23, "Full Heal", Items),
        new(24, "Revive", Items),
        new(25, "Max Revive", Items),
        new(34, "Ether", Items),
        new(36, "Elixir", Items),
        new(63, "HP Up", Items),
        new(64, "Protein", Items),
        new(65, "Iron", Items),
        new(66, "Carbos", Items),
        new(67, "Calcium", Items),
        new(68, "Rare Candy", Items),
        new(69, "PP Up", Items),
        new(70, "Zinc", Items),
        new(71, "PP Max", Items),
        new(93, "Sun Stone", Items),
        new(94, "Moon Stone", Items),
        new(95, "Fire Stone", Items),
        new(96, "Thunder Stone", Items),
        new(97, "Water Stone", Items),
        new(98, "Leaf Stone", Items),
    ];

    public static GameItem? Find(int id) => All.FirstOrDefault(item => item.Id == id);
}
