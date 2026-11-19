using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class GetMeEndpointSummary : EndpointSummary
{
    public GetMeEndpointSummary()
    {
        Summary = "Gets my trainer";
        Description = "Returns the current trainer and their starter, if chosen. Returns 404 for a few seconds after registration, until the trainer is created.";
    }
}
