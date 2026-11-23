namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>Something that happened to a trainer's Pokémon inside a specific game.</summary>
/// <param name="IsEvolution">It evolved in the game; otherwise it was caught there.</param>
/// <param name="SpeciesId">The species it was caught as, or evolved into.</param>
public sealed record GameFact(
    Guid TrainerId,
    Guid GameId,
    bool IsEvolution,
    Guid PokemonId,
    int SpeciesId,
    bool IsShiny,
    DateTime OccurredOnUtc);
