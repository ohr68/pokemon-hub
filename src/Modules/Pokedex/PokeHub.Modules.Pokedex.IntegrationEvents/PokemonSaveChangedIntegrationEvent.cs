using PokeHub.Common.Application.EventBus;

namespace PokeHub.Modules.Pokedex.IntegrationEvents;

/// <summary>
/// A Pokémon moved into or out of a trainer's save for a game, or the game delivered,
/// withdrew or released it.
/// </summary>
/// <param name="change">moved_in, moved_out, delivered, withdrawn, released or left_game.</param>
public sealed class PokemonSaveChangedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid pokemonId,
    Guid trainerId,
    int speciesId,
    Guid gameId,
    string change) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid PokemonId { get; init; } = pokemonId;

    public Guid TrainerId { get; init; } = trainerId;

    public int SpeciesId { get; init; } = speciesId;

    public Guid GameId { get; init; } = gameId;

    public string Change { get; init; } = change;
}
