using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Users.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);

    Task<Role> GetRoleAsync(Role role, CancellationToken cancellationToken = default);

    Task<bool> AnyWithRoleAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a role change in a transaction, one at a time, so two admins can't remove each
    /// other at once. A failed result rolls it back.
    /// </summary>
    Task<Result> WithRolesLockedAsync(Func<Task<Result>> change, CancellationToken cancellationToken = default);

    void Insert(User user);
}
