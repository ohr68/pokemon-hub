namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// A move from PokéAPI (Tackle, Ember, ...). Only moves some species learns in a game PokeHub
/// writes saves for are kept. The id is PokéAPI's, which Generation III games use as well.
/// </summary>
public sealed class Move
{
    private Move()
    {
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    // Power points: how many times the move can be used before resting.
    public int Pp { get; private set; }

    public static Move Create(int id, string name, int pp)
    {
        return new Move { Id = id, Name = name, Pp = pp };
    }

    public bool Update(string name, int pp)
    {
        if (Name == name && Pp == pp)
        {
            return false;
        }

        Name = name;
        Pp = pp;

        return true;
    }
}
