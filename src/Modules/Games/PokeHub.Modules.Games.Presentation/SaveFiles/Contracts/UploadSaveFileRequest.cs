using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Contracts;

internal sealed class UploadSaveFileRequest
{
    // Sent as multipart/form-data, in a field named "file".
    public IFormFile File { get; init; } = null!;
}
