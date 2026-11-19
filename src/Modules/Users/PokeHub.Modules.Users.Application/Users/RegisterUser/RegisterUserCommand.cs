using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Users.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<Guid>;
