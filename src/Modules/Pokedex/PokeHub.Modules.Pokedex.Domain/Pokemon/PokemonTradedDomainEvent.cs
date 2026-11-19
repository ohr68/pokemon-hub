using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public sealed class PokemonTradedDomainEvent(
    Guid pokemonId,
    Guid tradeId,
    Guid fromTrainerId,
    Guid toTrainerId,
    int fromSpeciesId,
    int toSpeciesId) : DomainEvent
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TradeId { get; init; } = tradeId;

    public Guid FromTrainerId { get; init; } = fromTrainerId;

    public Guid ToTrainerId { get; init; } = toTrainerId;

    public int FromSpeciesId { get; init; } = fromSpeciesId;

    // Differs from FromSpeciesId when the Pokémon evolved by trading.
    public int ToSpeciesId { get; init; } = toSpeciesId;
}
