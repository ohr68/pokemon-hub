using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Pokedex.Presentation.Species;

internal sealed class SpeciesEndpointGroup : Group
{
    public SpeciesEndpointGroup()
    {
        Configure("pokedex/species", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Pokédex: Species")
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}
