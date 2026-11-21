using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Users.Domain.Users;

/// <summary>A role was given or taken away. Kept in the outbox as the record of who did what.</summary>
public sealed class UserRoleChangedDomainEvent(Guid userId, string role, bool granted, Guid? changedBy) : DomainEvent
{
    public Guid UserId { get; init; } = userId;

    public string Role { get; init; } = role;

    public bool Granted { get; init; } = granted;

    // Empty for the first administrator, set up from configuration.
    public Guid? ChangedBy { get; init; } = changedBy;
}
