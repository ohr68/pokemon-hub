using FastEndpoints;

namespace PokeHub.Modules.Trades.Presentation.Trades.Docs;

internal sealed class RejectTradeEndpointSummary : EndpointSummary
{
    public RejectTradeEndpointSummary()
    {
        Summary = "Rejects a trade";
        Description = "Turns down an offer the current trainer received.";
    }
}
