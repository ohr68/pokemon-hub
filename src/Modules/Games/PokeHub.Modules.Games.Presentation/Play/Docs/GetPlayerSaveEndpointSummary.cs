using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Play.Docs;

internal sealed class GetPlayerSaveEndpointSummary : EndpointSummary
{
    public GetPlayerSaveEndpointSummary()
    {
        Summary = "Gets the player's save";
        Description = "Returns the signed-in player's save for the calling game, creating it on first use, with the Pokémon in it. Only accepts tokens from a registered game's client.";
    }
}
