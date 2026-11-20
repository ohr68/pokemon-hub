using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.SaveFiles.UploadSaveFile;

/// <param name="ClientId">The client the player signed in with, from the token.</param>
/// <param name="Content">The save file exactly as the game wrote it.</param>
public sealed record UploadSaveFileCommand(string? ClientId, Guid TrainerId, byte[] Content)
    : ICommand<UploadedSaveFileResponse>;

/// <param name="IsNew">False when the save matched the latest backup, which was kept instead.</param>
public sealed record UploadedSaveFileResponse(Guid Id, DateTime UploadedOnUtc, bool IsNew);
