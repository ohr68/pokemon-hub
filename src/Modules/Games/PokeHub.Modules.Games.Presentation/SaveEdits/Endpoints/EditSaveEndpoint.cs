using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.SaveEdits.EditSave;
using PokeHub.Modules.Games.Domain.SaveFiles;
using PokeHub.Modules.Games.Presentation.Play;
using PokeHub.Modules.Games.Presentation.SaveEdits.Contracts;
using PokeHub.Modules.Games.Presentation.SaveEdits.Docs;

namespace PokeHub.Modules.Games.Presentation.SaveEdits.Endpoints;

internal sealed class EditSaveEndpoint(ISender sender) : EndpointBase<EditSaveRequest>
{
    public override void Configure()
    {
        Post("save/edits");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();
        AllowFileUploads();

        Description(builder => builder
            .Accepts<EditSaveRequest>("multipart/form-data")
            .Produces<EditedSaveResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new EditSaveEndpointSummary());
    }

    public override async Task HandleAsync(EditSaveRequest req, CancellationToken ct)
    {
        // Checked before reading, so an oversized upload is never loaded into memory.
        if (req.File is null || req.File.Length > SaveFile.MaxSizeBytes)
        {
            await SendResultAsync(Result.Failure(SaveFileErrors.InvalidFile), Results.NoContent);
            return;
        }

        using var content = new MemoryStream((int)req.File.Length);
        await req.File.CopyToAsync(content, ct);

        Result<EditedSaveResponse> result = await sender.Send(
            new EditSaveCommand(User.GetClientId(), User.GetUserId(), content.ToArray(), req.Label),
            ct);

        await SendResultAsync(result, edited => Results.Ok(edited));
    }
}
