using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Games.Presentation.Play;

internal sealed class PlayEndpointGroup : Group
{
    public PlayEndpointGroup()
    {
        Configure("game", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Game API (for games)")
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesProblem(StatusCodes.Status409Conflict)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}
