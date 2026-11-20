namespace PokeHub.Modules.Pokedex.Application.Saves;

/// <param name="PokemonCount">Active Pokémon in the save right now.</param>
public sealed record SaveResponse(
    Guid Id,
    Guid GameId,
    string GameName,
    DateTime CreatedOnUtc,
    int PokemonCount);
