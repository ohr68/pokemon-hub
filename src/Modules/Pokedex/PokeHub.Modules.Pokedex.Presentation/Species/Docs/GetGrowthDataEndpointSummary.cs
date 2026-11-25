using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Docs;

internal sealed class GetGrowthDataEndpointSummary : EndpointSummary
{
    public GetGrowthDataEndpointSummary()
    {
        Summary = "Gets every species' growth rate";
        Description = "Returns every species' name and growth rate, and the growth-rate tables, to work out a boxed Pokémon's level from its experience in saves that store national numbers (Generation VI onwards). Open to everyone.";
    }
}
