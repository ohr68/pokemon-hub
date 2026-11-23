namespace PokeHub.Modules.Pokedex.Application.Saves;

/// <param name="PokemonCount">Active Pokémon in the save right now.</param>
/// <param name="MaxSpeciesId">The last National Dex number in its game; empty when it takes any Pokémon.</param>
public sealed record SaveResponse(
    Guid Id,
    Guid GameId,
    string GameName,
    DateTime CreatedOnUtc,
    int PokemonCount,
    int? MaxSpeciesId);
