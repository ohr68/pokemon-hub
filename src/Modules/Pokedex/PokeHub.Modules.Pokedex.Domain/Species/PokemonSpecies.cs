using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// A species in the catalog (Bulbasaur, Charmander, ...), kept in sync with PokéAPI.
/// The national Pokédex number is the identifier.
/// </summary>
public sealed class PokemonSpecies : Entity
{
    private PokemonSpecies()
    {
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public int Generation { get; private set; }

    public string PrimaryType { get; private set; }

    public string? SecondaryType { get; private set; }

    public DateTime SyncedOnUtc { get; private set; }

    public static PokemonSpecies Create(
        int id,
        string name,
        int generation,
        string primaryType,
        string? secondaryType,
        DateTime utcNow)
    {
        return new PokemonSpecies
        {
            Id = id,
            Name = name,
            Generation = generation,
            PrimaryType = primaryType,
            SecondaryType = secondaryType,
            SyncedOnUtc = utcNow,
        };
    }

    /// <summary>
    /// Applies data from a sync. Returns <c>true</c> when anything changed.
    /// </summary>
    public bool Update(string name, int generation, string primaryType, string? secondaryType, DateTime utcNow)
    {
        if (Name == name &&
            Generation == generation &&
            PrimaryType == primaryType &&
            SecondaryType == secondaryType)
        {
            return false;
        }

        Name = name;
        Generation = generation;
        PrimaryType = primaryType;
        SecondaryType = secondaryType;
        SyncedOnUtc = utcNow;

        return true;
    }
}
