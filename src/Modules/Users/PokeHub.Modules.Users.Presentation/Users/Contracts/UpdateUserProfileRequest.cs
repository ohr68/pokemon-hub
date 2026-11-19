namespace PokeHub.Modules.Users.Presentation.Users.Contracts;

internal sealed record UpdateUserProfileRequest(
    string FirstName,
    string LastName);
