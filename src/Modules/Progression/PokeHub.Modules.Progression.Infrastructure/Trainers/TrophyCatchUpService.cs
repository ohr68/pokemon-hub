using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Application.Trainers;

namespace PokeHub.Modules.Progression.Infrastructure.Trainers;

/// <summary>
/// Once at startup, gives trainers the trophies they already qualify for: trophies added in
/// this version reach trainers who met them before.
/// </summary>
internal sealed partial class TrophyCatchUpService(IServiceScopeFactory serviceScopeFactory, ILogger<TrophyCatchUpService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();

            Result<int> result = await scope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new CatchUpTrophiesCommand(), stoppingToken);

            LogCaughtUp(result.IsSuccess ? result.Value : 0);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Not worth stopping the API for: trainers get trophies with their next XP anyway.
            LogFailed(exception);
        }
    }

    [LoggerMessage(LogLevel.Information, "Trophy catch-up awarded {Count} trophies")]
    private partial void LogCaughtUp(int count);

    [LoggerMessage(LogLevel.Warning, "Trophy catch-up failed")]
    private partial void LogFailed(Exception exception);
}
