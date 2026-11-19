using System.Reflection;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PokeHub.Common.Presentation.Endpoints;

public static class EndpointExtensions
{
    public const string RoutePrefix = "api";

    /// <summary>
    /// Registers FastEndpoints once for the whole host, scanning only the given module
    /// presentation assemblies so endpoints never leak in from other assemblies.
    /// </summary>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly[] presentationAssemblies)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies = presentationAssemblies;
            options.DisableAutoDiscovery = true;
        });

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = RoutePrefix;
            config.Errors.UseProblemDetails();
        });

        return app;
    }
}
