using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.SaveEdits;

public static class SaveEditErrors
{
    public static readonly Error NotEditable = Error.Problem(
        "SaveEdits.NotEditable",
        "PokeHub doesn't edit this game's saves yet.");

    public static Error Unreadable(string reason)
    {
        return Error.Problem("SaveEdits.Unreadable", reason);
    }

    public static readonly Error NotInBoxes = Error.Conflict(
        "SaveEdits.NotInBoxes",
        "A chosen Pokémon is no longer in the PC boxes. Read the boxes again.");

    public static Error FromOtherGame(string name, string gameName)
    {
        return Error.Conflict("Pokemon.FromOtherGame", $"{name} was caught in another game and can't go to the hub through {gameName}.");
    }
}
