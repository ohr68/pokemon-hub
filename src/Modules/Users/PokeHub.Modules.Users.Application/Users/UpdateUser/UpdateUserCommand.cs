using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Users.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName) : ICommand;
