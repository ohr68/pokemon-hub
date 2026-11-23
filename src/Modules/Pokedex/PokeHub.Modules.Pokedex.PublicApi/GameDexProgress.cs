namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>How much of a game's own Pokédex a trainer has registered in their save for it.</summary>
/// <param name="Registered">Listed species caught in the game, evolved into in it, or sent to it.</param>
/// <param name="Total">Species in the game's Pokédex; zero when the game has no known list.</param>
public sealed record GameDexProgress(Guid TrainerId, Guid GameId, string GameName, int Registered, int Total);
