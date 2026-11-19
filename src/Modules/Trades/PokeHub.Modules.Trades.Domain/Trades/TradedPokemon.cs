namespace PokeHub.Modules.Trades.Domain.Trades;

/// <summary>
/// What a Pokémon looked like when it was put in a trade. Kept on the trade so offers can
/// be listed without the Pokédex and still read correctly after the Pokémon changes hands.
/// </summary>
public sealed record TradedPokemon(
    Guid PokemonId,
    int SpeciesId,
    string SpeciesName,
    string? Nickname,
    int Level,
    bool IsShiny);
