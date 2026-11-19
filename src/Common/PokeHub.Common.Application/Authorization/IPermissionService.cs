using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Common.Application.Authorization;

public interface IPermissionService
{
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}
