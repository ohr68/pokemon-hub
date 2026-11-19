using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class ReleasePokemonEndpointSummary : EndpointSummary
{
    public ReleasePokemonEndpointSummary()
    {
        Summary = "Releases one of my Pokémon";
        Description = "Removes the Pokémon from the current trainer. Its species stays in the trainer's Pokédex progress.";
    }
}
