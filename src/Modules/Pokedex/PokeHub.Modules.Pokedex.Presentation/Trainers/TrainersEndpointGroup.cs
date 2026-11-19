using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Pokedex.Presentation.Trainers;

internal sealed class TrainersEndpointGroup : Group
{
    public TrainersEndpointGroup()
    {
        Configure("pokedex/trainers", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Pokédex: Trainers")
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}
