using System.Reflection;
using FastEndpoints.Swagger;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PokeHub.Api.Extensions;
using PokeHub.Api.Middleware;
using PokeHub.Common.Application;
using PokeHub.Common.Infrastructure;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Infrastructure;
using PokeHub.Modules.Trades.Infrastructure;
using PokeHub.Modules.Users.Infrastructure;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

Assembly[] moduleApplicationAssemblies =
[
    PokeHub.Modules.Users.Application.AssemblyReference.Assembly,
    PokeHub.Modules.Pokedex.Application.AssemblyReference.Assembly,
    PokeHub.Modules.Trades.Application.AssemblyReference.Assembly,
];

// FastEndpoints is registered once for the host and only scans the modules' presentation assemblies.
Assembly[] modulePresentationAssemblies =
[
    PokeHub.Modules.Users.Presentation.AssemblyReference.Assembly,
    PokeHub.Modules.Pokedex.Presentation.AssemblyReference.Assembly,
    PokeHub.Modules.Trades.Presentation.AssemblyReference.Assembly,
];

builder.Services.AddApplication(moduleApplicationAssemblies);

string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;

// No module consumes integration events yet; add each module's ConfigureConsumers here when it does.
builder.Services.AddInfrastructure(
    [],
    databaseConnectionString,
    redisConnectionString);

builder.Configuration.AddModuleConfiguration(["users", "pokedex", "trades"]);

Uri keyCloakHealthUrl = builder.Configuration.GetKeyCloakHealthUrl();

builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString)
    .AddKeyCloak(keyCloakHealthUrl);

builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddPokedexModule(builder.Configuration);
builder.Services.AddTradesModule(builder.Configuration);

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

app.UseAuthentication();

app.UseAuthorization();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

await app.RunAsync();

#pragma warning disable CA1515
public partial class Program;
#pragma warning restore CA1515
