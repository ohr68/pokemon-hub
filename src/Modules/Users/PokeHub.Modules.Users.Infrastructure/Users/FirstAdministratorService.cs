using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PokeHub.Modules.Users.Application.Users.Roles;

namespace PokeHub.Modules.Users.Infrastructure.Users;

/// <summary>
/// At startup, when no administrator exists, gives the role to the account in
/// Users:FirstAdministratorEmail. Nobody could grant it otherwise.
/// </summary>
internal sealed partial class FirstAdministratorService(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    ILogger<FirstAdministratorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string? email = configuration["Users:FirstAdministratorEmail"];

        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();

            await scope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new GrantFirstAdministratorCommand(email.Trim()), stoppingToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Not worth stopping the API for: it tries again on the next start.
            LogFailed(exception);
        }
    }

    [LoggerMessage(LogLevel.Warning, "Could not set up the first administrator")]
    private partial void LogFailed(Exception exception);
}
