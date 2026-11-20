using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Docs;

internal sealed class GetSaveFileContentEndpointSummary : EndpointSummary
{
    public GetSaveFileContentEndpointSummary()
    {
        Summary = "Downloads a save file backup";
        Description = "Returns the backup's bytes, exactly as they were uploaded. Use latest for the player's current save.";
    }
}
