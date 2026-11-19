using FastEndpoints;

namespace PokeHub.Modules.Trades.Presentation.Trades.Docs;

internal sealed class GetMyTradesEndpointSummary : EndpointSummary
{
    public GetMyTradesEndpointSummary()
    {
        Summary = "Lists my trades";
        Description = "Returns the trades the current trainer proposed (Outgoing) or received (Incoming), newest first.";
    }
}
