using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Docs;

internal sealed class GetGen3DataEndpointSummary : EndpointSummary
{
    public GetGen3DataEndpointSummary()
    {
        Summary = "Gets the data to read Generation III saves";
        Description = "Returns each species' number inside Generation III games (it differs from the national number after #251) and the growth-rate tables, to work out levels from experience. Open to everyone.";
    }
}
