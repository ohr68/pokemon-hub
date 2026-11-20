using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.SaveFiles.GetSaveFiles;
using PokeHub.Modules.Games.Presentation.Play;
using PokeHub.Modules.Games.Presentation.SaveFiles.Docs;

namespace PokeHub.Modules.Games.Presentation.SaveFiles.Endpoints;

internal sealed class GetSaveFilesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("save/files");
        Policies(GamesPermissions.ReadPokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<SaveFileResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetSaveFilesEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<SaveFileResponse>> result = await sender.Send(
            new GetSaveFilesQuery(User.GetClientId(), User.GetUserId()),
            ct);

        await SendResultAsync(result, saveFiles => Results.Ok(saveFiles));
    }
}
