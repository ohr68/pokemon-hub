using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Games.Presentation.Games;

internal sealed class GamesEndpointGroup : Group
{
    public GamesEndpointGroup()
    {
        Configure("games", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Games")
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
