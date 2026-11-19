using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
