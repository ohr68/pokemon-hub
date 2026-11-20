using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Play.Docs;

internal sealed class ReconcileSaveEndpointSummary : EndpointSummary
{
    public ReconcileSaveEndpointSummary()
    {
        Summary = "Mirrors the game's save";
        Description = "Send the catch keys of every Pokémon in the player's save right now. Pokémon inside your game (caught there or delivered into it) that are still in its save on PokeHub but missing from that list left the game, and are removed. Use dryRun to see them first. Pokémon still waiting to be delivered are never touched.";
    }
}
