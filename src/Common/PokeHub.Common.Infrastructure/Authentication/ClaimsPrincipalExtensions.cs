using System.Security.Claims;
using PokeHub.Common.Application.Exceptions;

namespace PokeHub.Common.Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        public Guid GetUserId()
        {
            string? userId = principal?.FindFirst(CustomClaims.Sub)?.Value;

            return Guid.TryParse(userId, out Guid parsedUserId)
                ? parsedUserId
                : throw new PokeHubException("User identifier is unavailable.");
        }

        public string GetIdentityId()
        {
            return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   throw new PokeHubException("User identity is unavailable.");
        }

        public HashSet<string> GetPermissions()
        {
            IEnumerable<Claim> permissionClaims = principal?.FindAll(CustomClaims.Permission) ??
                                                  throw new PokeHubException("Permissions are unavailable.");

            return permissionClaims.Select(c => c.Value).ToHashSet();
        }
    }
}
