using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Pokedex.IntegrationEvents;

/// <summary>A Pokémon changed trainers in a trade; each trade publishes one per Pokémon.</summary>
public sealed class PokemonTradedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid pokemonId,
    Guid tradeId,
    Guid fromTrainerId,
    Guid toTrainerId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TradeId { get; init; } = tradeId;

    public Guid FromTrainerId { get; init; } = fromTrainerId;

    public Guid ToTrainerId { get; init; } = toTrainerId;
}
