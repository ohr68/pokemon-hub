using PokeHub.Modules.Users.Domain.Users;
using PokeHub.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Users.Infrastructure.Users;

internal sealed class UserRepository(UsersDbContext context) : IUserRepository
{
    // Any fixed number: it only has to be the same for every role change.
    private const long RolesLockKey = 7_301_001;

    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        return context.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<Role> GetRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        return context.Set<Role>().SingleAsync(r => r.Name == role.Name, cancellationToken);
    }

    public Task<bool> AnyWithRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(u => u.Roles.Any(r => r.Name == role.Name), cancellationToken);
    }

    public async Task<Result> WithRolesLockedAsync(Func<Task<Result>> change, CancellationToken cancellationToken = default)
    {
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Held until the transaction ends.
        await context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({RolesLockKey})", cancellationToken);

        Result result = await change();

        if (result.IsSuccess)
        {
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            await transaction.RollbackAsync(cancellationToken);
        }

        return result;
    }

    public void Insert(User user)
    {
        foreach (Role role in user.Roles)
        {
            context.Attach(role);
        }

        context.Users.Add(user);
    }
}
