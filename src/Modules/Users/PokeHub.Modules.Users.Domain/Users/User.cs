using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Users.Domain.Users;

public sealed class User : Entity
{
    private readonly List<Role> _roles = [];

    private User()
    {
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string IdentityId { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    public static User Create(string email, string firstName, string lastName, string identityId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IdentityId = identityId,
        };

        user._roles.Add(Role.Member);

        user.Raise(new UserRegisteredDomainEvent(user.Id));

        return user;
    }

    public bool HasRole(Role role) => _roles.Exists(r => r.Name == role.Name);

    /// <param name="changedBy">The admin who did it; empty for the first administrator, set up from configuration.</param>
    public void GrantRole(Role role, Guid? changedBy)
    {
        if (HasRole(role))
        {
            return;
        }

        _roles.Add(role);

        Raise(new UserRoleChangedDomainEvent(Id, role.Name, true, changedBy));
    }

    public void RevokeRole(Role role, Guid changedBy)
    {
        if (_roles.RemoveAll(r => r.Name == role.Name) > 0)
        {
            Raise(new UserRoleChangedDomainEvent(Id, role.Name, false, changedBy));
        }
    }

    public void Update(string firstName, string lastName)
    {
        if (FirstName == firstName && LastName == lastName)
        {
            return;
        }

        FirstName = firstName;
        LastName = lastName;

        Raise(new UserProfileUpdatedDomainEvent(Id, FirstName, LastName));
    }
}
