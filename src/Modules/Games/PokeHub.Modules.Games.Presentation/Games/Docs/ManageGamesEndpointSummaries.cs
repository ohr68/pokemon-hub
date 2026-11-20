using FastEndpoints;

namespace PokeHub.Modules.Games.Presentation.Games.Docs;

internal sealed class GetGameEndpointSummary : EndpointSummary
{
    public GetGameEndpointSummary()
    {
        Summary = "Gets an active game's page";
        Description = "The game's details, icon, cover and screenshot URLs (relative to the API root), and its ROM when PokeHub keeps one.";
    }
}

internal sealed class GetGameRomEndpointSummary : EndpointSummary
{
    public GetGameRomEndpointSummary()
    {
        Summary = "Downloads an active game's ROM";
        Description = "Signed-in players only. The ETag is the ROM's SHA-256, also in the game's page, so a cached copy can be reused.";
    }
}

internal sealed class GetGameImageEndpointSummary : EndpointSummary
{
    public GetGameImageEndpointSummary()
    {
        Summary = "Gets a game's cover, icon or screenshot";
        Description = "Public. The URL changes with the image, so it can be cached for good.";
    }
}

internal sealed class GetManagedGamesEndpointSummary : EndpointSummary
{
    public GetManagedGamesEndpointSummary()
    {
        Summary = "Lists every game, for admins";
        Description = "Turned-off games too, with their settings, webhook and files.";
    }
}

internal sealed class UpdateGameEndpointSummary : EndpointSummary
{
    public UpdateGameEndpointSummary()
    {
        Summary = "Updates a game's details and rate limit";
        Description = "The rate limit is how many game API requests one player may make per minute.";
    }
}

internal sealed class TurnGameOnOrOffEndpointSummary : EndpointSummary
{
    public TurnGameOnOrOffEndpointSummary()
    {
        Summary = "Turns a game on or off";
        Description = "A game turned off leaves the games list, its sign-in client is disabled, and the game API and CORS stop accepting it. Its players' Pokémon and saves stay.";
    }
}

internal sealed class UploadGameFileEndpointSummary : EndpointSummary
{
    public UploadGameFileEndpointSummary()
    {
        Summary = "Uploads a game's cover, icon, screenshot or ROM";
        Description = "Multipart/form-data with the file in a field named file. kind is cover, icon, rom or screenshots. A cover, icon or ROM replaces the current one. Images: PNG, JPEG, WebP or GIF up to 5 MB; ROMs up to 64 MB.";
    }
}

internal sealed class DeleteGameFileEndpointSummary : EndpointSummary
{
    public DeleteGameFileEndpointSummary()
    {
        Summary = "Deletes one of a game's files";
    }
}
