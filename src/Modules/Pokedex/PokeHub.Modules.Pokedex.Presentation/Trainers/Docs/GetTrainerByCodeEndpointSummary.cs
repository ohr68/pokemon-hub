using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Trainers.Docs;

internal sealed class GetTrainerByCodeEndpointSummary : EndpointSummary
{
    public GetTrainerByCodeEndpointSummary()
    {
        Summary = "Finds a trainer by code";
        Description = "Returns the trainer with the given public code (ABCD-EFGH, any case, dash optional) and the Pokémon they have now, to choose one to trade for.";
    }
}
