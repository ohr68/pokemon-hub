namespace PokeHub.Modules.Pokedex.Presentation;

// Seeded and granted to roles by the Users module (PermissionConfiguration).
internal static class PokedexPermissions
{
    internal const string ReadPokedex = "pokedex:read";
    internal const string UpdatePokedex = "pokedex:update";
    internal const string SyncSpecies = "species:sync";
    internal const string GrantPokemon = "pokemon:grant";
}
