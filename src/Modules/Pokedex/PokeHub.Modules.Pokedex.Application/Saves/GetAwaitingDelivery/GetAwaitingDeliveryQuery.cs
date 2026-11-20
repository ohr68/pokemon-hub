using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.GetAwaitingDelivery;

/// <summary>
/// Pokémon the trainer moved into their save for a game on PokeHub, which the game still has
/// to write into its own save.
/// </summary>
public sealed record GetAwaitingDeliveryQuery(Guid TrainerId, Guid GameId) : IQuery<IReadOnlyCollection<AwaitingDeliveryResponse>>;

/// <param name="IsOriginalTrainer">The trainer is the Pokémon's original trainer.</param>
/// <param name="GameData">The Pokémon as the last game that held it stored it, if any.</param>
public sealed record AwaitingDeliveryResponse(
    Guid Id,
    int SpeciesId,
    string SpeciesName,
    string? Nickname,
    int Level,
    bool IsShiny,
    bool IsOriginalTrainer,
    Guid OriginalTrainerId,
    string OriginalTrainerName,
    string? GameDataFormat,
    byte[]? GameData);
