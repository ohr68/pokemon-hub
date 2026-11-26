namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// An item from PokéAPI (Master Ball, Leftovers, ...) that Generation III or the 3DS games
/// have. Saves don't use PokéAPI's id: each game generation numbers items its own way, so the
/// item keeps both numbers a synced save may hold.
/// </summary>
public sealed class Item
{
    private Item()
    {
    }

    // PokéAPI's id.
    public int Id { get; private set; }

    public string Name { get; private set; }

    // Its number in Ruby, Sapphire, Emerald, FireRed and LeafGreen.
    public int? Gen3Index { get; private set; }

    // Its number in X, Y, Sun, Moon, Ultra Sun and Ultra Moon, which number items the same way.
    public int? ModernIndex { get; private set; }

    public static Item Create(int id, string name, int? gen3Index, int? modernIndex)
    {
        return new Item { Id = id, Name = name, Gen3Index = gen3Index, ModernIndex = modernIndex };
    }

    public bool Update(string name, int? gen3Index, int? modernIndex)
    {
        if (Name == name && Gen3Index == gen3Index && ModernIndex == modernIndex)
        {
            return false;
        }

        Name = name;
        Gen3Index = gen3Index;
        ModernIndex = modernIndex;

        return true;
    }
}
