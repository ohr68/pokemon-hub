using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Docs;

internal sealed class GetSaveFilesEndpointSummary : EndpointSummary
{
    public GetSaveFilesEndpointSummary()
    {
        Summary = "Lists the player's save file backups";
        Description = "The player's backups for the calling game, newest first, without their content.";
    }
}
