namespace PokeHub.Modules.Games.Presentation;

// Seeded and granted to roles by the Users module (PermissionConfiguration).
internal static class GamesPermissions
{
    internal const string ManageGames = "games:manage";

    // Game calls act on the player's Pokédex, so they use the Pokédex permissions.
    internal const string ReadPokedex = "pokedex:read";
    internal const string UpdatePokedex = "pokedex:update";
}
