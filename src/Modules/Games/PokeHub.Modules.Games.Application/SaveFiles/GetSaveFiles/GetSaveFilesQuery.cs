using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.SaveFiles.GetSaveFiles;

/// <param name="ClientId">The client the player signed in with, from the token.</param>
public sealed record GetSaveFilesQuery(string? ClientId, Guid TrainerId) : IQuery<IReadOnlyCollection<SaveFileResponse>>;

/// <param name="Label">Where it came from when it wasn't a sync.</param>
public sealed record SaveFileResponse(Guid Id, int SizeBytes, string Sha256, DateTime UploadedOnUtc, string? Label);
