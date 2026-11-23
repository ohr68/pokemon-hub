using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.GameFiles;
using PokeHub.Modules.Games.Application.Games.ManageGames;
using PokeHub.Modules.Games.Application.Games.TurnGameOnOrOff;
using PokeHub.Modules.Games.Application.Games.UpdateGame;
using PokeHub.Modules.Games.Domain.GameFiles;
using PokeHub.Modules.Games.Presentation.Games.Contracts;
using PokeHub.Modules.Games.Presentation.Games.Docs;

namespace PokeHub.Modules.Games.Presentation.Games.Endpoints;

internal sealed class GetManagedGamesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("manage");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<ManagedGameResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetManagedGamesEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<ManagedGameResponse>> result = await sender.Send(new GetManagedGamesQuery(), ct);

        await SendResultAsync(result, games => Results.Ok(games));
    }
}

internal sealed class GetManagedGameEndpoint(ISender sender) : EndpointBase<GameRouteRequest>
{
    public override void Configure()
    {
        Get("manage/{gameId:guid}");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<ManagedGameResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetManagedGamesEndpointSummary());
    }

    public override async Task HandleAsync(GameRouteRequest req, CancellationToken ct)
    {
        Result<IReadOnlyCollection<ManagedGameResponse>> result = await sender.Send(new GetManagedGamesQuery(req.GameId), ct);

        await SendResultAsync(result, games => Results.Ok(games.Single()));
    }
}

internal sealed class UpdateGameEndpoint(ISender sender) : EndpointBase<UpdateGameRequest>
{
    public override void Configure()
    {
        Put("{gameId:guid}");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new UpdateGameEndpointSummary());
    }

    public override async Task HandleAsync(UpdateGameRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new UpdateGameCommand(req.GameId, req.Name, req.Description, req.RateLimitPerMinute, req.MaxSpeciesId),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class TurnGameOnEndpoint(ISender sender) : EndpointBase<GameRouteRequest>
{
    public override void Configure()
    {
        Post("{gameId:guid}/turn-on");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new TurnGameOnOrOffEndpointSummary());
    }

    public override async Task HandleAsync(GameRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new TurnGameOnOrOffCommand(req.GameId, On: true), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class TurnGameOffEndpoint(ISender sender) : EndpointBase<GameRouteRequest>
{
    public override void Configure()
    {
        Post("{gameId:guid}/turn-off");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new TurnGameOnOrOffEndpointSummary());
    }

    public override async Task HandleAsync(GameRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new TurnGameOnOrOffCommand(req.GameId, On: false), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class UploadGameFileEndpoint(ISender sender) : EndpointBase<UploadGameFileRequest>
{
    private static readonly Dictionary<string, GameFileKind> Kinds = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cover"] = GameFileKind.Cover,
        ["icon"] = GameFileKind.Icon,
        ["rom"] = GameFileKind.Rom,
        ["screenshots"] = GameFileKind.Screenshot,
    };

    public override void Configure()
    {
        Post("{gameId:guid}/files/{kind}");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();
        AllowFileUploads();

        Description(builder => builder
            .Accepts<UploadGameFileRequest>("multipart/form-data")
            .Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(new UploadGameFileEndpointSummary());
    }

    public override async Task HandleAsync(UploadGameFileRequest req, CancellationToken ct)
    {
        if (!Kinds.TryGetValue(req.Kind, out GameFileKind kind) || req.File is null)
        {
            await SendResultAsync(Result.Failure(GameFileErrors.Empty), Results.NoContent);
            return;
        }

        int maxSize = kind is GameFileKind.Rom ? GameFile.MaxRomSizeBytes : GameFile.MaxImageSizeBytes;

        // Checked before reading, so an oversized upload is never loaded into memory.
        if (req.File.Length > maxSize)
        {
            await SendResultAsync(Result.Failure(GameFileErrors.TooLarge(maxSize)), Results.NoContent);
            return;
        }

        using var content = new MemoryStream((int)req.File.Length);
        await req.File.CopyToAsync(content, ct);

        Result<Guid> result = await sender.Send(
            new UploadGameFileCommand(req.GameId, kind, req.File.FileName, req.File.ContentType, content.ToArray()),
            ct);

        await SendResultAsync(result, id => Results.Ok(id));
    }
}

internal sealed class DeleteGameFileEndpoint(ISender sender) : EndpointBase<GameFileRouteRequest>
{
    public override void Configure()
    {
        Delete("{gameId:guid}/files/{fileId:guid}");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new DeleteGameFileEndpointSummary());
    }

    public override async Task HandleAsync(GameFileRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new DeleteGameFileCommand(req.GameId, req.FileId), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
