namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

/// <summary>
/// One entry of a Pokémon's ownership history. Records are only ever added, never changed.
/// The species is copied here so counts per species don't need to join the Pokémon.
/// </summary>
public sealed class OwnershipRecord
{
    private OwnershipRecord()
    {
    }

    public Guid Id { get; private set; }

    public Guid PokemonId { get; private set; }

    public Guid TrainerId { get; private set; }

    public int SpeciesId { get; private set; }

    public OwnershipChange Change { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    internal static OwnershipRecord Create(
        Guid pokemonId,
        Guid trainerId,
        int speciesId,
        OwnershipChange change,
        DateTime utcNow)
    {
        return new OwnershipRecord
        {
            Id = Guid.CreateVersion7(),
            PokemonId = pokemonId,
            TrainerId = trainerId,
            SpeciesId = speciesId,
            Change = change,
            OccurredOnUtc = utcNow,
        };
    }
}
