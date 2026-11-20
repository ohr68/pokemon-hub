using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Progression.Presentation.Trainers;

internal sealed class ProgressionEndpointGroup : Group
{
    public ProgressionEndpointGroup()
    {
        Configure("progression", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Progression")
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}
