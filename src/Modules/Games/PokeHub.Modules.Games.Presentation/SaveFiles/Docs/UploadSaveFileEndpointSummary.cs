using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Docs;

internal sealed class UploadSaveFileEndpointSummary : EndpointSummary
{
    public UploadSaveFileEndpointSummary()
    {
        Summary = "Backs up the player's save file";
        Description = "Send the save file exactly as the game wrote it, as multipart/form-data in a field named file (at most 1 MB), with an optional label (where it came from, such as an imported file). Each upload is kept as a backup and the newest is the player's current save; the 20 newest are kept. Sending the same save as the latest backup stores nothing new (isNew is false).";
    }
}
