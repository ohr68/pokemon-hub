using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.SaveFiles.GetSaveFileContent;

/// <param name="ClientId">The client the player signed in with, from the token.</param>
/// <param name="SaveFileId">A backup's id, or null for the latest one.</param>
public sealed record GetSaveFileContentQuery(string? ClientId, Guid TrainerId, Guid? SaveFileId)
    : IQuery<SaveFileContentResponse>;

public sealed record SaveFileContentResponse(Guid Id, string GameSlug, DateTime UploadedOnUtc, byte[] Content);
