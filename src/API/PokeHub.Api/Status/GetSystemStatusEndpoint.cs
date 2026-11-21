using System.Data.Common;
using Dapper;
using FastEndpoints;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PokeHub.Common.Application.Data;

namespace PokeHub.Api.Status;

internal sealed record ServiceStatusResponse(string Name, string Status);

internal sealed record ModuleMessagesResponse(string Module, long Failed, long Stuck);

internal sealed record MessageFailureResponse(string Module, string Kind, string Event, string Error, DateTime OccurredOnUtc);

internal sealed record WebhookFailureResponse(string Game, string EventType, int Attempts, int? StatusCode, string? Error, DateTime CreatedOnUtc);

internal sealed record BackupsResponse(DateTime? DatabaseUtc, DateTime? IdentityUtc);

internal sealed record SystemStatusResponse(
    IReadOnlyCollection<ServiceStatusResponse> Services,
    IReadOnlyCollection<ModuleMessagesResponse> Modules,
    IReadOnlyCollection<MessageFailureResponse> RecentFailures,
    long WebhooksRetrying,
    IReadOnlyCollection<WebhookFailureResponse> WebhooksGivenUp,
    BackupsResponse Backups,
    int WindowDays);

/// <summary>
/// What needs attention across the app. It lives in the host because it reads every module's
/// outbox and inbox; it only reads, and changes nothing.
/// </summary>
internal sealed class GetSystemStatusEndpoint(
    HealthCheckService healthChecks,
    IDbConnectionFactory dbConnectionFactory,
    IConfiguration configuration) : EndpointWithoutRequest<SystemStatusResponse>
{
    private const int WindowDays = 7;
    private const int MaxListed = 20;
    private const int MaxErrorLength = 300;

    // A message waiting longer than this isn't being picked up by its job.
    private const int StuckAfterMinutes = 5;

    private static readonly string[] Schemas = ["users", "pokedex", "trades", "games", "progression", "economy"];

    private static readonly string[] MessageKinds = ["outbox", "inbox"];

    public override void Configure()
    {
        Get("system/status");
        Policies("system:read");
        Description(builder => builder.WithTags("System"));
        Summary(s => s.Summary = "Service health, failed messages and webhooks, and the last backups");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        HealthReport health = await healthChecks.CheckHealthAsync(ct);
        ServiceStatusResponse[] services =
        [
            .. health.Entries.Select(entry => new ServiceStatusResponse(entry.Key, entry.Value.Status.ToString())),
        ];

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        IEnumerable<ModuleRow> modules = await connection.QueryAsync<ModuleRow>(ModulesSql(), new { WindowDays, StuckAfterMinutes });
        IEnumerable<FailureRow> failures = await connection.QueryAsync<FailureRow>(FailuresSql(), new { WindowDays, MaxListed });
        long retrying = await connection.ExecuteScalarAsync<long>(
            "SELECT count(*) FROM games.webhook_deliveries WHERE delivered_on_utc IS NULL AND next_attempt_on_utc IS NOT NULL");
        IEnumerable<WebhookRow> givenUp = await connection.QueryAsync<WebhookRow>(WebhooksSql, new { WindowDays, MaxListed });

        string statusFolder = configuration["Backups:StatusPath"] ?? "/backup-status";

        await Send.OkAsync(
            new SystemStatusResponse(
                services,
                [.. modules.Select(row => new ModuleMessagesResponse(row.Module, row.Failed, row.Stuck))],
                [.. failures.Select(row => new MessageFailureResponse(row.Module, row.Kind, EventName(row.Type), Summarize(row.Error), row.OccurredOnUtc))],
                retrying,
                [.. givenUp.Select(row => new WebhookFailureResponse(row.Game, row.EventType, row.Attempts, row.StatusCode, row.Error, row.CreatedOnUtc))],
                new BackupsResponse(
                    await ReadBackupTimeAsync(Path.Combine(statusFolder, "database"), ct),
                    await ReadBackupTimeAsync(Path.Combine(statusFolder, "identity"), ct)),
                WindowDays),
            ct);
    }

    private static string MessagesSql() => string.Join(
        " UNION ALL ",
        Schemas.SelectMany(schema => MessageKinds.Select(kind =>
            $"SELECT '{schema}' AS module, '{kind}' AS kind, type, error, occurred_on_utc, processed_on_utc FROM {schema}.{kind}_messages")));

    private static string ModulesSql() =>
        $"""
         SELECT module AS Module,
                count(*) FILTER (WHERE error IS NOT NULL AND occurred_on_utc >= now() - make_interval(days => @WindowDays)) AS Failed,
                count(*) FILTER (WHERE processed_on_utc IS NULL AND occurred_on_utc < now() - make_interval(mins => @StuckAfterMinutes)) AS Stuck
         FROM ({MessagesSql()}) messages
         GROUP BY module
         ORDER BY module
         """;

    private static string FailuresSql() =>
        $"""
         SELECT module AS Module, kind AS Kind, type AS Type, error AS Error, occurred_on_utc AS OccurredOnUtc
         FROM ({MessagesSql()}) messages
         WHERE error IS NOT NULL AND occurred_on_utc >= now() - make_interval(days => @WindowDays)
         ORDER BY occurred_on_utc DESC
         LIMIT @MaxListed
         """;

    private const string WebhooksSql =
        """
        SELECT g.slug AS Game, d.event_type AS EventType, d.attempts AS Attempts,
               d.last_status_code AS StatusCode, d.last_error AS Error, d.created_on_utc AS CreatedOnUtc
        FROM games.webhook_deliveries d
        JOIN games.games g ON g.id = d.game_id
        WHERE d.delivered_on_utc IS NULL AND d.next_attempt_on_utc IS NULL
          AND d.created_on_utc >= now() - make_interval(days => @WindowDays)
        ORDER BY d.created_on_utc DESC
        LIMIT @MaxListed
        """;

    // "Some.Namespace.PokemonSaveChangedIntegrationEvent, Some.Assembly" -> "PokemonSaveChanged"
    private static string EventName(string type)
    {
        string name = type.Split(',')[0].Split('.')[^1];

        return name.Replace("IntegrationEvent", string.Empty, StringComparison.Ordinal)
            .Replace("DomainEvent", string.Empty, StringComparison.Ordinal);
    }

    // The innermost exception says what went wrong; the outer ones only wrap it.
    private static string Summarize(string error)
    {
        string[] lines = error.Split('\n', StringSplitOptions.TrimEntries);
        string line = lines.LastOrDefault(l => l.StartsWith("--->", StringComparison.Ordinal))?[4..].Trim() ?? lines[0];

        return line.Length > MaxErrorLength ? line[..MaxErrorLength] : line;
    }

    private static async Task<DateTime?> ReadBackupTimeAsync(string path, CancellationToken ct)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        string text = await File.ReadAllTextAsync(path, ct);

        return DateTime.TryParse(
            text.Trim(),
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AdjustToUniversal,
            out DateTime value)
            ? value
            : null;
    }

    private sealed record ModuleRow(string Module, long Failed, long Stuck);

    private sealed record FailureRow(string Module, string Kind, string Type, string Error, DateTime OccurredOnUtc);

    private sealed record WebhookRow(string Game, string EventType, int Attempts, int? StatusCode, string? Error, DateTime CreatedOnUtc);
}
