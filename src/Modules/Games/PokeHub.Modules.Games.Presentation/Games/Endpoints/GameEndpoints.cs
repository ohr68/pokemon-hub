using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.GameFiles;
using PokeHub.Modules.Games.Application.Games.GetGame;
using PokeHub.Modules.Games.Presentation.Games.Contracts;
using PokeHub.Modules.Games.Presentation.Games.Docs;

namespace PokeHub.Modules.Games.Presentation.Games.Endpoints;

internal sealed class GetGameEndpoint(ISender sender) : EndpointBase<GameSlugRequest>
{
    public override void Configure()
    {
        Get("{slug}");
        AllowAnonymous();
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<GameDetailsResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetGameEndpointSummary());
    }

    public override async Task HandleAsync(GameSlugRequest req, CancellationToken ct)
    {
        Result<GameDetailsResponse> result = await sender.Send(new GetGameQuery(req.Slug), ct);

        await SendResultAsync(result, game => Results.Ok(game));
    }
}

internal sealed class GetGameRomEndpoint(ISender sender) : EndpointBase<GameSlugRequest>
{
    public override void Configure()
    {
        Get("{slug}/rom");
        // Any signed-in player.
        Policies(GamesPermissions.ReadPokedex);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status200OK, contentType: "application/octet-stream"));
        Summary(new GetGameRomEndpointSummary());
    }

    public override async Task HandleAsync(GameSlugRequest req, CancellationToken ct)
    {
        Result<GameFileContentResponse> result = await sender.Send(new GetGameFileContentQuery(RomOfSlug: req.Slug), ct);

        await SendResultAsync(result, rom => Results.File(
            rom.Content,
            rom.ContentType,
            rom.FileName,
            entityTag: new EntityTagHeaderValue($"\"{rom.Sha256}\"")));
    }
}

internal sealed class GetGameImageEndpoint(ISender sender) : EndpointBase<GameFileRouteRequest>
{
    public override void Configure()
    {
        Get("files/{fileId:guid}");
        AllowAnonymous();
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status200OK, contentType: "image/png"));
        Summary(new GetGameImageEndpointSummary());
    }

    public override async Task HandleAsync(GameFileRouteRequest req, CancellationToken ct)
    {
        Result<GameFileContentResponse> result = await sender.Send(new GetGameFileContentQuery(ImageId: req.FileId), ct);

        if (result.IsSuccess)
        {
            // Image URLs carry the file's hash, so a URL's content never changes.
            HttpContext.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }

        await SendResultAsync(result, image => Results.File(image.Content, image.ContentType));
    }
}
