using FastEndpoints;

namespace PokeHub.Modules.Trades.Presentation.Trades.Docs;

internal sealed class ProposeTradeEndpointSummary : EndpointSummary
{
    public ProposeTradeEndpointSummary()
    {
        Summary = "Proposes a trade";
        Description = "Offers one of the current trainer's Pokémon for one of another trainer's (found by their trainer code). The offer waits for an answer for 7 days.";
    }
}
