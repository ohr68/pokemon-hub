using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PokeHub.Modules.Games.PublicApi;

namespace PokeHub.Modules.Games.Presentation.Play;

internal sealed class PlayEndpointGroup : Group
{
    public PlayEndpointGroup()
    {
        Configure("game", endpoint =>
        {
            // Each player gets the game's own number of requests per minute.
            endpoint.Options(builder => builder.RequireRateLimiting(GameRateLimiting.PolicyName));

            endpoint.Description(builder => builder
                    .WithTags("Game API (for games)")
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesProblem(StatusCodes.Status409Conflict)
                    .ProducesProblem(StatusCodes.Status429TooManyRequests)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}
