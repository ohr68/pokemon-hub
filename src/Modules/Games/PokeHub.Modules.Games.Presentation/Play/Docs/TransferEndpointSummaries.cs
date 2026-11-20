using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Play.Docs;

internal sealed class GetDeliveriesEndpointSummary : EndpointSummary
{
    public GetDeliveriesEndpointSummary()
    {
        Summary = "Lists Pokémon waiting to go into the game";
        Description = "Pokémon the player moved into this game's save on PokeHub. Write them into the game's own save, then confirm with POST save/deliveries. gameData holds the Pokémon exactly as a game stored it before (base64, in gameDataFormat), when it was in a game already: a game that reads the format should write it back unchanged.";
    }
}

internal sealed class DeliverPokemonEndpointSummary : EndpointSummary
{
    public DeliverPokemonEndpointSummary()
    {
        Summary = "Confirms Pokémon were written into the game";
        Description = "Send each delivered Pokémon with the catch key the game knows it by now. All are recorded, or none. Repeating a delivery changes nothing.";
    }
}

internal sealed class WithdrawPokemonEndpointSummary : EndpointSummary
{
    public WithdrawPokemonEndpointSummary()
    {
        Summary = "Sends Pokémon from the game to the hub";
        Description = "For Pokémon the game took out of its own save. Each is found by its catch key and moves to the hub with its species, level and nickname now, and the game's own bytes for it (base64 data in dataFormat, such as pk3), written back if it returns. All move, or none. Pokémon inside a game can only leave it this way.";
    }
}
