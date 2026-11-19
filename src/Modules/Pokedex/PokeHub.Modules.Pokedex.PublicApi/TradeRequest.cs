namespace PokeHub.Modules.Pokedex.PublicApi;

public sealed record TradeRequest(
    Guid TradeId,
    Guid FirstTrainerId,
    Guid FirstPokemonId,
    Guid SecondTrainerId,
    Guid SecondPokemonId);
