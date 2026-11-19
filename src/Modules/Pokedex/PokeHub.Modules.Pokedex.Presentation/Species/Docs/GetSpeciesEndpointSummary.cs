using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Docs;

internal sealed class GetSpeciesEndpointSummary : EndpointSummary
{
    public GetSpeciesEndpointSummary()
    {
        Summary = "Searches the species catalog";
        Description = "Lists species by Pokédex number, filtered by part of the name or the exact number, type and generation. Open to everyone.";
    }
}
