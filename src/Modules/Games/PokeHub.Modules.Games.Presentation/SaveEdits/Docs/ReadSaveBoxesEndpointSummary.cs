using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.SaveEdits.Docs;

internal sealed class ReadSaveBoxesEndpointSummary : EndpointSummary
{
    public ReadSaveBoxesEndpointSummary()
    {
        Summary = "Lists the Pokémon in a save's PC boxes";
        Description = "For a device that can't read the game's save itself, such as the 3DS app. Send the save file as multipart/form-data in a field named file. Returns each PC box Pokémon with its box, slot, name, level and catch key, to send back in the withdraw field of save/edits. Changes and stores nothing. Only Gen 3 games for now.";
    }
}
