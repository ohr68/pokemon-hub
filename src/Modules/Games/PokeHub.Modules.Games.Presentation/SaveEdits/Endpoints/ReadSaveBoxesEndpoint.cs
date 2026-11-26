using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.SaveEdits.ReadSaveBoxes;
using PokeHub.Modules.Games.Domain.SaveFiles;
using PokeHub.Modules.Games.Presentation.Play;
using PokeHub.Modules.Games.Presentation.SaveEdits.Contracts;
using PokeHub.Modules.Games.Presentation.SaveEdits.Docs;

namespace PokeHub.Modules.Games.Presentation.SaveEdits.Endpoints;

internal sealed class ReadSaveBoxesEndpoint(ISender sender) : EndpointBase<EditSaveRequest>
{
    public override void Configure()
    {
        Post("save/boxes");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();
        AllowFileUploads();

        Description(builder => builder
            .Accepts<EditSaveRequest>("multipart/form-data")
            .Produces<SaveBoxesResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new ReadSaveBoxesEndpointSummary());
    }

    public override async Task HandleAsync(EditSaveRequest req, CancellationToken ct)
    {
        if (req.File is null || req.File.Length > SaveFile.MaxSizeBytes)
        {
            await SendResultAsync(Result.Failure(SaveFileErrors.InvalidFile), Results.NoContent);
            return;
        }

        using var content = new MemoryStream((int)req.File.Length);
        await req.File.CopyToAsync(content, ct);

        Result<SaveBoxesResponse> result = await sender.Send(new ReadSaveBoxesQuery(User.GetClientId(), content.ToArray()), ct);

        await SendResultAsync(result, boxes => Results.Ok(boxes));
    }
}
