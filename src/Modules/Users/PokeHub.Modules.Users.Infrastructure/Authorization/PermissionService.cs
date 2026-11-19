using PokeHub.Common.Application.Authorization;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Users.Application.Users.GetUserPermissions;
using MediatR;

namespace PokeHub.Modules.Users.Infrastructure.Authorization;

internal sealed class PermissionService(ISender sender) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        return await sender.Send(new GetUserPermissionsQuery(identityId));
    }
}
