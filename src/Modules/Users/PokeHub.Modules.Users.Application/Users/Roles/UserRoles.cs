using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Users.Application.Abstractions.Data;
using PokeHub.Modules.Users.Domain.Users;

namespace PokeHub.Modules.Users.Application.Users.Roles;

public sealed record GetUsersWithRolesQuery : IQuery<IReadOnlyCollection<UserWithRolesResponse>>;

public sealed record UserWithRolesResponse(Guid Id, string Email, string FirstName, string LastName, bool IsAdministrator);

/// <param name="ActorId">The administrator making the change.</param>
public sealed record SetAdministratorCommand(Guid ActorId, Guid UserId, bool IsAdministrator) : ICommand;

/// <summary>
/// Makes this account the first administrator, only while there are none, so the setting can't
/// be used to add another one later.
/// </summary>
public sealed record GrantFirstAdministratorCommand(string Email) : ICommand;

internal sealed class GetUsersWithRolesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetUsersWithRolesQuery, IReadOnlyCollection<UserWithRolesResponse>>
{
    public async Task<Result<IReadOnlyCollection<UserWithRolesResponse>>> Handle(
        GetUsersWithRolesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT u.id AS Id, u.email AS Email, u.first_name AS FirstName, u.last_name AS LastName,
                   EXISTS (
                       SELECT 1 FROM users.user_roles r
                       WHERE r.user_id = u.id AND r.roles_name = 'Administrator'
                   ) AS IsAdministrator
            FROM users.users u
            ORDER BY IsAdministrator DESC, u.first_name, u.email
            """;

        IEnumerable<UserWithRolesResponse> users = await connection.QueryAsync<UserWithRolesResponse>(sql);

        return users.ToList();
    }
}

/// <summary>
/// Nobody can remove their own role, and the one acting must still be an administrator once the
/// lock is held. Together they mean there is always at least one administrator left.
/// </summary>
internal sealed class SetAdministratorCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<SetAdministratorCommand>
{
    public Task<Result> Handle(SetAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdministrator && request.ActorId == request.UserId)
        {
            return Task.FromResult(Result.Failure(UserErrors.CannotRevokeOwnAdministrator));
        }

        return userRepository.WithRolesLockedAsync(
            async () =>
            {
                User? actor = await userRepository.GetWithRolesAsync(request.ActorId, cancellationToken);

                if (actor is null || !actor.HasRole(Role.Administrator))
                {
                    return Result.Failure(UserErrors.NoLongerAdministrator);
                }

                User? user = await userRepository.GetWithRolesAsync(request.UserId, cancellationToken);

                if (user is null)
                {
                    return Result.Failure(UserErrors.NotFound(request.UserId));
                }

                Role administrator = await userRepository.GetRoleAsync(Role.Administrator, cancellationToken);

                if (request.IsAdministrator)
                {
                    user.GrantRole(administrator, request.ActorId);
                }
                else
                {
                    user.RevokeRole(administrator, request.ActorId);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            },
            cancellationToken);
    }
}

internal sealed class GrantFirstAdministratorCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<GrantFirstAdministratorCommand>
{
    public Task<Result> Handle(GrantFirstAdministratorCommand request, CancellationToken cancellationToken)
    {
        return userRepository.WithRolesLockedAsync(
            async () =>
            {
                if (await userRepository.AnyWithRoleAsync(Role.Administrator, cancellationToken))
                {
                    return Result.Success();
                }

                User? user = await userRepository.GetByEmailWithRolesAsync(request.Email, cancellationToken);

                if (user is null)
                {
                    return Result.Success();
                }

                user.GrantRole(await userRepository.GetRoleAsync(Role.Administrator, cancellationToken), changedBy: null);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            },
            cancellationToken);
    }
}
