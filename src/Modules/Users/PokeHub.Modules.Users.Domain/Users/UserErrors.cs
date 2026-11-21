using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Users.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId)
    {
        return Error.NotFound("Users.NotFound", $"The user with the identifier {userId} was not found.");
    }

    public static readonly Error CannotRevokeOwnAdministrator = Error.Conflict(
        "Users.CannotRevokeOwnAdministrator",
        "You can't remove your own administrator role. Another administrator has to do it.");

    public static readonly Error NoLongerAdministrator = Error.Conflict(
        "Users.NoLongerAdministrator",
        "You are no longer an administrator.");

    public static Error NotFound(string identityId)
    {
        return Error.NotFound("Users.NotFound", $"The user with the IDP identifier {identityId} was not found.");
    }
}
