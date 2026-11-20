using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.SaveFiles.GetSaveFileContent;
using PokeHub.Modules.Games.Presentation.Play;
using PokeHub.Modules.Games.Presentation.SaveFiles.Contracts;
using PokeHub.Modules.Games.Presentation.SaveFiles.Docs;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Endpoints;

internal sealed class GetSaveFileContentEndpoint(ISender sender) : EndpointBase<SaveFileRouteRequest>
{
    public override void Configure()
    {
        Get("save/files/latest/content", "save/files/{saveFileId:guid}/content");
        Policies(GamesPermissions.ReadPokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status200OK, contentType: "application/octet-stream"));
        Summary(new GetSaveFileContentEndpointSummary());
    }

    public override async Task HandleAsync(SaveFileRouteRequest req, CancellationToken ct)
    {
        Result<SaveFileContentResponse> result = await sender.Send(
            new GetSaveFileContentQuery(User.GetClientId(), User.GetUserId(), req.SaveFileId),
            ct);

        await SendResultAsync(result, saveFile => Results.File(
            saveFile.Content,
            "application/octet-stream",
            $"{saveFile.GameSlug}-{saveFile.UploadedOnUtc.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture)}.sav"));
    }
}
