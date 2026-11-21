using System.Reflection;
using FastEndpoints.Swagger;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PokeHub.Api.Extensions;
using PokeHub.Api.Middleware;
using PokeHub.Common.Application;
using PokeHub.Common.Infrastructure;
using PokeHub.Common.Presentation.Endpoints;
using Microsoft.AspNetCore.Cors.Infrastructure;
using PokeHub.Modules.Games.Infrastructure;
using PokeHub.Modules.Games.PublicApi;
using PokeHub.Modules.Pokedex.Infrastructure;
using PokeHub.Modules.Progression.Infrastructure;
using PokeHub.Modules.Economy.Infrastructure;
using PokeHub.Modules.Trades.Infrastructure;
using PokeHub.Modules.Users.Infrastructure;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// Game ROMs are uploaded through the API (up to 64 MB).
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 80 * 1024 * 1024);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Browser apps (the pokemon-web frontend) call the API from another origin.
// Allowed: the configured origins, plus the origins of every registered game, which can
// change at runtime as games are registered.
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors();
builder.Services.AddOptions<CorsOptions>()
    .Configure<IGameOriginPolicy>((options, gameOrigins) => options.AddDefaultPolicy(policy => policy
        .SetIsOriginAllowed(origin => allowedOrigins.Contains(origin) || gameOrigins.IsAllowed(origin))
        .AllowAnyHeader()
        .AllowAnyMethod()
        // Lets browser games read how long to wait after 429, and a ROM's version.
        .WithExposedHeaders("Retry-After", "ETag")));

builder.Services.AddGameRateLimiting();

Assembly[] moduleApplicationAssemblies =
[
    PokeHub.Modules.Users.Application.AssemblyReference.Assembly,
    PokeHub.Modules.Pokedex.Application.AssemblyReference.Assembly,
    PokeHub.Modules.Trades.Application.AssemblyReference.Assembly,
    PokeHub.Modules.Games.Application.AssemblyReference.Assembly,
    PokeHub.Modules.Progression.Application.AssemblyReference.Assembly,
    PokeHub.Modules.Economy.Application.AssemblyReference.Assembly,
];

// FastEndpoints is registered once for the host and scans the modules' presentation assemblies,
// plus the host itself for the system status, which spans every module.
Assembly[] modulePresentationAssemblies =
[
    typeof(Program).Assembly,
    PokeHub.Modules.Users.Presentation.AssemblyReference.Assembly,
    PokeHub.Modules.Pokedex.Presentation.AssemblyReference.Assembly,
    PokeHub.Modules.Trades.Presentation.AssemblyReference.Assembly,
    PokeHub.Modules.Games.Presentation.AssemblyReference.Assembly,
    PokeHub.Modules.Progression.Presentation.AssemblyReference.Assembly,
    PokeHub.Modules.Economy.Presentation.AssemblyReference.Assembly,
];

builder.Services.AddApplication(moduleApplicationAssemblies);

string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;

// Each module that consumes integration events registers its consumers here.
builder.Services.AddInfrastructure(
    [
        PokedexModule.ConfigureConsumers,
        GamesModule.ConfigureConsumers,
        ProgressionModule.ConfigureConsumers,
        EconomyModule.ConfigureConsumers,
    ],
    databaseConnectionString,
    redisConnectionString);

builder.Configuration.AddModuleConfiguration(["users", "pokedex", "trades", "games", "progression", "economy"]);

Uri keyCloakHealthUrl = builder.Configuration.GetKeyCloakHealthUrl();

builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString)
    .AddKeyCloak(keyCloakHealthUrl);

builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddPokedexModule(builder.Configuration);
builder.Services.AddTradesModule(builder.Configuration);
builder.Services.AddGamesModule(builder.Configuration);
builder.Services.AddProgressionModule(builder.Configuration);
builder.Services.AddEconomyModule(builder.Configuration);

builder.Services.AddEndpoints(modulePresentationAssemblies);
builder.Services.AddSwaggerDocumentation();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

// After authentication: the limits are per game and player, read from the token.
app.UseRateLimiter();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

await app.RunAsync();

#pragma warning disable CA1515
public partial class Program;
#pragma warning restore CA1515
