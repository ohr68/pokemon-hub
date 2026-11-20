using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class MovePokemonEndpointSummary : EndpointSummary
{
    public MovePokemonEndpointSummary()
    {
        Summary = "Moves one of my Pokémon between the hub and a game";
        Description = "Moves a Pokémon from the hub into one of the trainer's game saves, or back to the hub when saveId is empty. Moving between two games goes through the hub.";
    }
}
