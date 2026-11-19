using PokeHub.Common.Application.Authorization;
using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Users.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
