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

    public static readonly Error NotFound = Error.NotFound(
        "SaveEdits.NotFound",
        "There's no such edit of your save in this game.");

    public static readonly Error AlreadySettled = Error.Conflict(
        "SaveEdits.AlreadySettled",
        "This edit was already settled by a later save.");

    public static readonly Error NotInBoxes = Error.Conflict(
        "SaveEdits.NotInBoxes",
        "A chosen Pokémon is no longer in the PC boxes. Read the boxes again.");

    public static Error FromOtherGame(string name, string gameName)
    {
        return Error.Conflict("Pokemon.FromOtherGame", $"{name} was caught in another game and can't go to the hub through {gameName}.");
    }
}
