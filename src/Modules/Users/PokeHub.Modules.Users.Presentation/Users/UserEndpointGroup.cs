using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Users.Presentation.Users;

internal sealed class UserEndpointGroup : Group
{
    public UserEndpointGroup()
    {
        Configure("users", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Users")
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}
