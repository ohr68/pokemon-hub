using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// A species in the catalog (Bulbasaur, Charmander, ...), kept in sync with PokéAPI.
/// The national Pokédex number is the identifier.
/// </summary>
public sealed class PokemonSpecies : Entity
{
    private readonly List<LevelUpMove> _gen3LevelUpMoves = [];

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

    /// <summary>
    /// The moves the species learns by level in FireRed and LeafGreen, used to give a Pokémon
    /// sent into those games the moves it would know. Empty for species introduced later.
    /// </summary>
    public IReadOnlyCollection<LevelUpMove> Gen3LevelUpMoves => _gen3LevelUpMoves.AsReadOnly();

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
        IReadOnlyCollection<LevelUpMove> gen3LevelUpMoves,
        DateTime utcNow)
    {
        var species = new PokemonSpecies
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

        species._gen3LevelUpMoves.AddRange(gen3LevelUpMoves);

        return species;
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
        IReadOnlyCollection<LevelUpMove> gen3LevelUpMoves,
        DateTime utcNow)
    {
        if (Name == name &&
            Generation == generation &&
            PrimaryType == primaryType &&
            SecondaryType == secondaryType &&
            TradeEvolutionSpeciesId == tradeEvolutionSpeciesId &&
            GrowthRate == growthRate &&
            Gen3Index == gen3Index &&
            _gen3LevelUpMoves.SequenceEqual(gen3LevelUpMoves))
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

        if (!_gen3LevelUpMoves.SequenceEqual(gen3LevelUpMoves))
        {
            _gen3LevelUpMoves.Clear();
            _gen3LevelUpMoves.AddRange(gen3LevelUpMoves);
        }

        return true;
    }
}
