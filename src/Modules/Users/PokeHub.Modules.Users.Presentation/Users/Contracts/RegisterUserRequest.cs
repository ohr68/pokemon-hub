namespace PokeHub.Modules.Users.Presentation.Users.Contracts;

internal sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);
