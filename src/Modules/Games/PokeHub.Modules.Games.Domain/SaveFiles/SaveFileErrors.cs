using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.SaveFiles;

public static class SaveFileErrors
{
    public static Error NotFound(Guid saveFileId)
    {
        return Error.NotFound("SaveFiles.NotFound", $"No save file {saveFileId} was found for you in this game.");
    }

    public static readonly Error InvalidFile = Error.Problem(
        "SaveFiles.InvalidFile",
        $"Send one save file of at most {SaveFile.MaxSizeBytes / 1024} KB in a field named file.");

    public static readonly Error NoneYet = Error.NotFound(
        "SaveFiles.NoneYet",
        "There is no save file on PokeHub for you in this game yet.");
}
