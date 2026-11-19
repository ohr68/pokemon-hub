namespace PokeHub.Modules.Users.IntegrationTests.Abstractions;

// Mirrors the register endpoint's JSON body. The endpoint's own contract is internal to the
// presentation project, so tests describe the request they send themselves.
internal sealed class RegisterUserRequest
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }
}
