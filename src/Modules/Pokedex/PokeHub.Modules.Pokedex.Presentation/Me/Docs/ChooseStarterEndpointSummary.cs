using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class ChooseStarterEndpointSummary : EndpointSummary
{
    public ChooseStarterEndpointSummary()
    {
        Summary = "Chooses the starter";
        Description = "Gives the current trainer their starter at level 5. Each trainer chooses once, from the grass, fire and water starters of every generation.";
    }
}
