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

    /// <summary>
    /// The species this one becomes when traded without a held item (Kadabra into Alakazam, ...).
    /// </summary>
    public int? TradeEvolutionSpeciesId { get; private set; }

    /// <summary>
    /// How much experience the species needs per level (slow, medium, fast, ...), see <see cref="GrowthRate"/>.
    /// </summary>
    public string? GrowthRate { get; private set; }

    /// <summary>
    /// The species' number inside Generation III games (Ruby to FireRed), which differs from
    /// the national number after #251. Empty for species introduced later.
    /// </summary>
    public int? Gen3Index { get; private set; }

    public DateTime SyncedOnUtc { get; private set; }

    public static PokemonSpecies Create(
        int id,
        string name,
        int generation,
        string primaryType,
        string? secondaryType,
        int? tradeEvolutionSpeciesId,
        string? growthRate,
        int? gen3Index,
        DateTime utcNow)
    {
        return new PokemonSpecies
        {
            Id = id,
            Name = name,
            Generation = generation,
            PrimaryType = primaryType,
            SecondaryType = secondaryType,
            TradeEvolutionSpeciesId = tradeEvolutionSpeciesId,
            GrowthRate = growthRate,
            Gen3Index = gen3Index,
            SyncedOnUtc = utcNow,
        };
    }

    /// <summary>
    /// Applies data from a sync. Returns <c>true</c> when anything changed.
    /// </summary>
    public bool Update(
        string name,
        int generation,
        string primaryType,
        string? secondaryType,
        int? tradeEvolutionSpeciesId,
        string? growthRate,
        int? gen3Index,
        DateTime utcNow)
    {
        if (Name == name &&
            Generation == generation &&
            PrimaryType == primaryType &&
            SecondaryType == secondaryType &&
            TradeEvolutionSpeciesId == tradeEvolutionSpeciesId &&
            GrowthRate == growthRate &&
            Gen3Index == gen3Index)
        {
            return false;
        }

        Name = name;
        Generation = generation;
        PrimaryType = primaryType;
        SecondaryType = secondaryType;
        TradeEvolutionSpeciesId = tradeEvolutionSpeciesId;
        GrowthRate = growthRate;
        Gen3Index = gen3Index;
        SyncedOnUtc = utcNow;

        return true;
    }
}
