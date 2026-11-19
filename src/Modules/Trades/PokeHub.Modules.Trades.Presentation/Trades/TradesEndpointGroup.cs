using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Trades.Presentation.Trades;

internal sealed class TradesEndpointGroup : Group
{
    public TradesEndpointGroup()
    {
        Configure("trades", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Trades")
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
