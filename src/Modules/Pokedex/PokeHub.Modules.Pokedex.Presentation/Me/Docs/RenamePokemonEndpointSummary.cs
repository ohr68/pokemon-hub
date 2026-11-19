using FastEndpoints;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Docs;

internal sealed class RenamePokemonEndpointSummary : EndpointSummary
{
    public RenamePokemonEndpointSummary()
    {
        Summary = "Renames one of my Pokémon";
        Description = "Sets the nickname of one of the current trainer's Pokémon, up to 12 characters. An empty nickname removes it.";
    }
}
