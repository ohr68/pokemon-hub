using Microsoft.EntityFrameworkCore;
using Npgsql;
using PokeHub.Modules.Games.Infrastructure.Database;
using PokeHub.Modules.Progression.Infrastructure.Database;
using PokeHub.Modules.Economy.Infrastructure.Database;
using PokeHub.Modules.Pokedex.Infrastructure.Database;
using PokeHub.Modules.Trades.Infrastructure.Database;
using PokeHub.Modules.Users.Infrastructure.Database;

namespace PokeHub.Api.Extensions;

internal static partial class MigrationExtensions
{
    private static readonly TimeSpan DatabaseRetryDelay = TimeSpan.FromSeconds(3);

    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        WaitForDatabase(scope);

        ApplyMigration<UsersDbContext>(scope);
        ApplyMigration<PokedexDbContext>(scope);
        ApplyMigration<TradesDbContext>(scope);
        ApplyMigration<GamesDbContext>(scope);
        ApplyMigration<ProgressionDbContext>(scope);
        ApplyMigration<EconomyDbContext>(scope);
    }

    // After a Docker restart the containers come up in no particular order, so the API may be
    // first. It waits here instead of failing on the first migration.
    private static void WaitForDatabase(IServiceScope scope)
    {
        UsersDbContext context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        while (!CanConnect(context))
        {
            LogWaitingForDatabase(logger, DatabaseRetryDelay.TotalSeconds);
            Thread.Sleep(DatabaseRetryDelay);
        }
    }

    private static bool CanConnect(DbContext context)
    {
        try
        {
            return context.Database.CanConnect();
        }
        catch (NpgsqlException)
        {
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Database not reachable yet, trying again in {Seconds} seconds")]
    private static partial void LogWaitingForDatabase(ILogger logger, double seconds);

    private static void ApplyMigration<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }
}
