using System.Data.Common;
using Dapper;
using FluentValidation;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.Webhooks;

/// <param name="Url">Where to send the game's events; empty stops them.</param>
public sealed record SetWebhookCommand(Guid GameId, Uri? Url) : ICommand;

public sealed record RotateWebhookSecretCommand(Guid GameId) : ICommand;

public sealed record GetWebhookDeliveriesQuery(Guid GameId) : IQuery<IReadOnlyCollection<WebhookDeliveryResponse>>;

/// <param name="Status">delivered, pending, retrying or failed.</param>
public sealed record WebhookDeliveryResponse(
    Guid Id,
    string EventType,
    DateTime CreatedOnUtc,
    string Status,
    int Attempts,
    int? LastStatusCode,
    string? LastError,
    DateTime? NextAttemptOnUtc,
    DateTime? DeliveredOnUtc);

internal sealed class SetWebhookCommandValidator : AbstractValidator<SetWebhookCommand>
{
    public SetWebhookCommandValidator()
    {
        RuleFor(c => c.Url)
            .Must(url => url is null || url.IsAbsoluteUri && url.Scheme is "http" or "https")
            .WithMessage("The webhook URL must be an http(s) URL.");
    }
}

internal sealed class SetWebhookCommandHandler(IGameRepository gameRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<SetWebhookCommand>
{
    public async Task<Result> Handle(SetWebhookCommand request, CancellationToken cancellationToken)
    {
        Game? game = await gameRepository.GetAsync(request.GameId, cancellationToken);

        if (game is null)
        {
            return Result.Failure(GameErrors.NotFound(request.GameId));
        }

        game.SetWebhook(request.Url);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class RotateWebhookSecretCommandHandler(IGameRepository gameRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RotateWebhookSecretCommand>
{
    public async Task<Result> Handle(RotateWebhookSecretCommand request, CancellationToken cancellationToken)
    {
        Game? game = await gameRepository.GetAsync(request.GameId, cancellationToken);

        if (game is null)
        {
            return Result.Failure(GameErrors.NotFound(request.GameId));
        }

        Result rotated = game.RotateWebhookSecret();

        if (rotated.IsFailure)
        {
            return rotated;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class GetWebhookDeliveriesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetWebhookDeliveriesQuery, IReadOnlyCollection<WebhookDeliveryResponse>>
{
    public async Task<Result<IReadOnlyCollection<WebhookDeliveryResponse>>> Handle(
        GetWebhookDeliveriesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT
                id AS Id,
                event_type AS EventType,
                created_on_utc AS CreatedOnUtc,
                CASE
                    WHEN delivered_on_utc IS NOT NULL THEN 'delivered'
                    WHEN next_attempt_on_utc IS NULL THEN 'failed'
                    WHEN attempts = 0 THEN 'pending'
                    ELSE 'retrying'
                END AS Status,
                attempts AS Attempts,
                last_status_code AS LastStatusCode,
                last_error AS LastError,
                next_attempt_on_utc AS NextAttemptOnUtc,
                delivered_on_utc AS DeliveredOnUtc
            FROM games.webhook_deliveries
            WHERE game_id = @GameId
            ORDER BY created_on_utc DESC
            LIMIT 50
            """;

        IEnumerable<WebhookDeliveryResponse> deliveries = await connection.QueryAsync<WebhookDeliveryResponse>(
            sql,
            new { request.GameId });

        return deliveries.ToList();
    }
}
