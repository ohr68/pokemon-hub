using FastEndpoints;

namespace PokeHub.Modules.Trades.Presentation.Trades.Docs;

internal sealed class AcceptTradeEndpointSummary : EndpointSummary
{
    public AcceptTradeEndpointSummary()
    {
        Summary = "Accepts a trade";
        Description = "Swaps the two Pokémon right away. Pokémon that evolve by trading evolve, and both sides keep their original trainer. Fails with 409 if a Pokémon is no longer available or the offer expired.";
    }
}
