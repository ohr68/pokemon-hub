using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.GetOrCreateSave;

/// <summary>
/// The trainer's save for a game, created the first time they play it.
/// </summary>
public sealed record GetOrCreateSaveCommand(Guid TrainerId, Guid GameId, string GameName) : ICommand<SaveResponse>;
