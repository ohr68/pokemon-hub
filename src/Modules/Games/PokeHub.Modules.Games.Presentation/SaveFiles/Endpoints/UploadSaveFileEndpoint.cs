using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.SaveFiles.UploadSaveFile;
using PokeHub.Modules.Games.Domain.SaveFiles;
using PokeHub.Modules.Games.Presentation.Play;
using PokeHub.Modules.Games.Presentation.SaveFiles.Contracts;
using PokeHub.Modules.Games.Presentation.SaveFiles.Docs;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Endpoints;

internal sealed class UploadSaveFileEndpoint(ISender sender) : EndpointBase<UploadSaveFileRequest>
{
    public override void Configure()
    {
        Post("save/files");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();
        AllowFileUploads();

        Description(builder => builder
            .Accepts<UploadSaveFileRequest>("multipart/form-data")
            .Produces<UploadedSaveFileResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new UploadSaveFileEndpointSummary());
    }

    public override async Task HandleAsync(UploadSaveFileRequest req, CancellationToken ct)
    {
        // Checked before reading, so an oversized upload is never loaded into memory.
        if (req.File is null || req.File.Length > SaveFile.MaxSizeBytes)
        {
            await SendResultAsync(Result.Failure(SaveFileErrors.InvalidFile), Results.NoContent);
            return;
        }

        using var content = new MemoryStream((int)req.File.Length);
        await req.File.CopyToAsync(content, ct);

        Result<UploadedSaveFileResponse> result = await sender.Send(
            new UploadSaveFileCommand(User.GetClientId(), User.GetUserId(), content.ToArray()),
            ct);

        await SendResultAsync(result, uploaded => Results.Ok(uploaded));
    }
}
