using FastEndpoints;

namespace PokeHub.Modules.Trades.Presentation.Trades.Docs;

internal sealed class CancelTradeEndpointSummary : EndpointSummary
{
    public CancelTradeEndpointSummary()
    {
        Summary = "Cancels a trade";
        Description = "Withdraws an offer the current trainer made, while it's still waiting for an answer.";
    }
}
