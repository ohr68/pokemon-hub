using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Webhooks;
using PokeHub.Modules.Games.Presentation.Games;
using PokeHub.Modules.Games.Presentation.Games.Contracts;

namespace PokeHub.Modules.Games.Presentation.Webhooks;

internal sealed class SetWebhookRequest
{
    public Guid GameId { get; init; }

    // Empty stops the game's events.
    public Uri? Url { get; init; }
}

internal sealed class SetWebhookEndpoint(ISender sender) : EndpointBase<SetWebhookRequest>
{
    public override void Configure()
    {
        Put("{gameId:guid}/webhook");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Sets or removes a game's webhook";
            s.Description = "PokeHub sends the game's events there: pokemon.moved_in, moved_out, delivered, withdrawn, released and left_game. A secret for checking signatures is created the first time.";
        });
    }

    public override async Task HandleAsync(SetWebhookRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new SetWebhookCommand(req.GameId, req.Url), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class RotateWebhookSecretEndpoint(ISender sender) : EndpointBase<GameRouteRequest>
{
    public override void Configure()
    {
        Post("{gameId:guid}/webhook/rotate-secret");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s => s.Summary = "Replaces a game's webhook secret");
    }

    public override async Task HandleAsync(GameRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new RotateWebhookSecretCommand(req.GameId), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class SendTestWebhookEndpoint(ISender sender) : EndpointBase<GameRouteRequest>
{
    public override void Configure()
    {
        Post("{gameId:guid}/webhook/test");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Sends a ping event to a game's webhook";
            s.Description = "Queues a ping event, sent within seconds; follow it in the deliveries.";
        });
    }

    public override async Task HandleAsync(GameRouteRequest req, CancellationToken ct)
    {
        Result<Guid?> result = await sender.Send(
            new QueueWebhookEventCommand(req.GameId, "ping", new { message = "Hello from PokeHub" }),
            ct);

        await SendResultAsync(result, id => id is null
            ? Results.Problem(title: "Games.NoWebhook", detail: "The game has no webhook, or is turned off.", statusCode: StatusCodes.Status400BadRequest)
            : Results.Ok(id));
    }
}

internal sealed class GetWebhookDeliveriesEndpoint(ISender sender) : EndpointBase<GameRouteRequest>
{
    public override void Configure()
    {
        Get("{gameId:guid}/webhook/deliveries");
        Policies(GamesPermissions.ManageGames);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<WebhookDeliveryResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "Lists a game's latest 50 webhook deliveries");
    }

    public override async Task HandleAsync(GameRouteRequest req, CancellationToken ct)
    {
        Result<IReadOnlyCollection<WebhookDeliveryResponse>> result = await sender.Send(new GetWebhookDeliveriesQuery(req.GameId), ct);

        await SendResultAsync(result, deliveries => Results.Ok(deliveries));
    }
}
