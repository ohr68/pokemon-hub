using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Docs;

internal sealed class SyncSpeciesEndpointSummary : EndpointSummary
{
    public SyncSpeciesEndpointSummary()
    {
        Summary = "Syncs the catalog with PokéAPI";
        Description = "Starts the species sync job now and returns right away. The same job also runs on its weekly schedule. Requires the species:sync permission.";
    }
}
