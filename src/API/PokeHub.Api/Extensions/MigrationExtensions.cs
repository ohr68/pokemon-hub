using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Pokedex.Infrastructure.Database;
using PokeHub.Modules.Trades.Infrastructure.Database;
using PokeHub.Modules.Users.Infrastructure.Database;

namespace PokeHub.Api.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplyMigration<UsersDbContext>(scope);
        ApplyMigration<PokedexDbContext>(scope);
        ApplyMigration<TradesDbContext>(scope);
    }

    private static void ApplyMigration<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }
}
