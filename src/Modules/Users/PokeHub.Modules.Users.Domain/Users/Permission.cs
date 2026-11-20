namespace PokeHub.Modules.Users.Domain.Users;

public sealed class Permission
{
    public static readonly Permission GetUser = new("users:read");
    public static readonly Permission ModifyUser = new("users:update");
    public static readonly Permission ReadPokedex = new("pokedex:read");
    public static readonly Permission UpdatePokedex = new("pokedex:update");
    public static readonly Permission SyncSpecies = new("species:sync");
    public static readonly Permission GrantPokemon = new("pokemon:grant");
    public static readonly Permission ReadTrades = new("trades:read");
    public static readonly Permission UpdateTrades = new("trades:update");
    public static readonly Permission ManageGames = new("games:manage");
    public static readonly Permission ReadProgression = new("progression:read");

    public Permission(string code)
    {
        Code = code;
    }

    public string Code { get; private set; }
}
