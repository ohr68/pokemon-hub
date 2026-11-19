using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Trainers.Docs;

internal sealed class GrantPokemonEndpointSummary : EndpointSummary
{
    public GrantPokemonEndpointSummary()
    {
        Summary = "Gives a Pokémon to a trainer";
        Description = "Creates a Pokémon for any trainer. An admin tool until games can give Pokémon. Requires the pokemon:grant permission.";
    }
}
