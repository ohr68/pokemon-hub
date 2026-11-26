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
}
