using MediatR;
using Microsoft.Extensions.Logging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Species.SyncSpecies;
using Quartz;

namespace PokeHub.Modules.Pokedex.Infrastructure.SpeciesSync;

/// <summary>
/// Runs the species sync. Started by the cron schedule, once on startup for an empty catalog,
/// and by the manual sync endpoint. Never runs twice at the same time.
/// </summary>
[DisallowConcurrentExecution]
internal sealed partial class SyncSpeciesJob(ISender sender, ILogger<SyncSpeciesJob> logger) : IJob
{
    public static readonly JobKey Key = new(typeof(SyncSpeciesJob).FullName!);

    public const string OnlyIfEmptyKey = "OnlyIfEmpty";

    public async Task Execute(IJobExecutionContext context)
    {
        bool onlyIfEmpty = context.MergedJobDataMap.TryGetBoolean(OnlyIfEmptyKey, out bool value) && value;

        Result<SpeciesSyncResponse> result = await sender.Send(
            new SyncSpeciesCommand(onlyIfEmpty),
            context.CancellationToken);

        if (result.IsFailure)
        {
            LogSyncFailed(result.Error.Code, result.Error.Description);
            return;
        }

        SpeciesSyncResponse sync = result.Value;

        if (sync.Skipped)
        {
            LogSyncSkipped();
            return;
        }

        LogSyncCompleted(sync.Added, sync.Updated, sync.Total);
    }

    [LoggerMessage(LogLevel.Information, "Species sync completed: {Added} added, {Updated} updated, {Total} in PokéAPI")]
    private partial void LogSyncCompleted(int added, int updated, int total);

    [LoggerMessage(LogLevel.Information, "Species sync skipped: the catalog already has species")]
    private partial void LogSyncSkipped();

    [LoggerMessage(LogLevel.Error, "Species sync failed: {ErrorCode} {ErrorDescription}")]
    private partial void LogSyncFailed(string errorCode, string errorDescription);
}
