namespace PokeHub.Modules.Pokedex.PublicApi;

public sealed record SaveInfo(Guid Id, Guid GameId, string GameName, DateTime CreatedOnUtc);
