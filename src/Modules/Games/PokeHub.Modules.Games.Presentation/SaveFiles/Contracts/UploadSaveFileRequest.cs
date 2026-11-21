using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Contracts;

internal sealed class UploadSaveFileRequest
{
    // Sent as multipart/form-data, in a field named "file".
    public IFormFile File { get; init; } = null!;

    // Optional, in a field named label: where the save came from, such as an imported file.
    public string? Label { get; init; }
}
