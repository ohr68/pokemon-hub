using Microsoft.AspNetCore.Http;

namespace PokeHub.Modules.Games.Presentation.SaveEdits.Contracts;

internal sealed class EditSaveRequest
{
    // Sent as multipart/form-data, in a field named "file".
    public IFormFile File { get; init; } = null!;

    // Optional, in a field named label: where the save came from, such as "Do 3DS".
    public string? Label { get; init; }
}
