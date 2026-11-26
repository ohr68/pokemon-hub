using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.SaveEdits.Docs;

internal sealed class EditSaveEndpointSummary : EndpointSummary
{
    public EditSaveEndpointSummary()
    {
        Summary = "Puts the Pokémon waiting for the game into its save";
        Description = "For a device that plays the game outside PokeHub, such as the 3DS app. Send the save file exactly as the game wrote it, as multipart/form-data in a field named file, with an optional label. PokeHub backs it up, puts back to waiting any Pokémon an earlier edit delivered that aren't in it, writes the Pokémon waiting for the game into free PC slots, records them as delivered and returns the edited save (content, base64) for the device to write into the game; changed is false when there was nothing to put in. Only Gen 3 games for now.";
    }
}
