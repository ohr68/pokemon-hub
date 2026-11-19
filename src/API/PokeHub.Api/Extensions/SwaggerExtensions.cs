using FastEndpoints.Swagger;

namespace PokeHub.Api.Extensions;

internal static class SwaggerExtensions
{
    internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.SwaggerDocument(options =>
        {
            options.DocumentSettings = settings =>
            {
                settings.Title = "PokeHub API";
                settings.Version = "v1";
                settings.Description = "PokeHub API built using the modular monolith architecture.";
            };

            // Keycloak access tokens are sent as bearer tokens.
            options.EnableJWTBearerAuth = true;
            options.ShortSchemaNames = true;
        });

        return services;
    }
}
