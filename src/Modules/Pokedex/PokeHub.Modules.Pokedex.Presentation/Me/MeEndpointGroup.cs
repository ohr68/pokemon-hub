using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Pokedex.Presentation.Me;

internal sealed class MeEndpointGroup : Group
{
    public MeEndpointGroup()
    {
        Configure("pokedex/me", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Pokédex: My Pokémon")
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}
