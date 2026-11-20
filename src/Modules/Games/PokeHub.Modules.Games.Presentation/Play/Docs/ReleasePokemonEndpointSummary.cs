using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Play.Docs;

internal sealed class ReleasePokemonEndpointSummary : EndpointSummary
{
    public ReleasePokemonEndpointSummary()
    {
        Summary = "Releases a Pokémon from the save";
        Description = "Releases a Pokémon that is in the player's save for the calling game.";
    }
}
