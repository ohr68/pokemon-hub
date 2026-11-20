using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Play.Docs;

internal sealed class CatchPokemonEndpointSummary : EndpointSummary
{
    public CatchPokemonEndpointSummary()
    {
        Summary = "Records a catch";
        Description = "Adds a Pokémon caught in the game to the player's save. Send a catchKey that identifies the catch in your game: repeating a request with the same key returns the same Pokémon instead of a second one. An optional nickname (up to 12 characters) keeps the name the player gave it in the game.";
    }
}
