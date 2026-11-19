using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Docs;

internal sealed class GetSpeciesByIdEndpointSummary : EndpointSummary
{
    public GetSpeciesByIdEndpointSummary()
    {
        Summary = "Gets one species";
        Description = "Returns the species with the given national Pokédex number. Open to everyone.";
    }
}
