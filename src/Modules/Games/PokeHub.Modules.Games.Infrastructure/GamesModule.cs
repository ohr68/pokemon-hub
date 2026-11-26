using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.GameFiles;
using PokeHub.Modules.Games.Domain.SaveEdits;
using PokeHub.Modules.Games.Domain.SaveFiles;
using PokeHub.Modules.Games.Domain.Stats;
using PokeHub.Modules.Games.Domain.Webhooks;
using PokeHub.Modules.Games.Infrastructure.GameFiles;
using PokeHub.Modules.Games.Infrastructure.Webhooks;
using PokeHub.Modules.Games.Infrastructure.Database;
using PokeHub.Modules.Games.Infrastructure.Games;
using PokeHub.Modules.Games.Infrastructure.Identity;
using PokeHub.Modules.Games.Infrastructure.SaveEdits;
using PokeHub.Modules.Games.Infrastructure.SaveFiles;
using PokeHub.Modules.Games.Infrastructure.Stats;
using PokeHub.Modules.Games.PublicApi;
using Microsoft.Extensions.Options;
using PokeHub.Modules.Games.Infrastructure.Inbox;
using PokeHub.Modules.Games.Infrastructure.Outbox;
using PokeHub.Modules.Games.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MassTransit;
using PokeHub.Modules.Pokedex.IntegrationEvents;

namespace PokeHub.Modules.Games.Infrastructure;

public static class GamesModule
{
    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        // The Pokédex publishes PokemonSaveChanged; the inbox turns each into a webhook event.
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<PokemonSaveChangedIntegrationEvent>>();
    }

    extension(IServiceCollection services)
    {
        public void AddGamesModule(IConfiguration configuration)
        {
            services.AddDomainEventHandlers();

            services.AddIntegrationEventHandlers();

            services.AddInfrastructure(configuration);
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddDbContext<GamesDbContext>((sp, options) =>
                options.UseNpgsql(configuration.GetConnectionString("Database"),
                        npgsqlOptions => npgsqlOptions
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Games))
                    .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<GamesDbContext>());

            services.Configure<OutboxOptions>(configuration.GetSection("Games:Outbox"));

            services.ConfigureOptions<ConfigureProcessOutboxJob>();

            services.Configure<InboxOptions>(configuration.GetSection("Games:Inbox"));

            services.ConfigureOptions<ConfigureProcessInboxJob>();

            services.AddScoped<IGameRepository, GameRepository>();

            services.AddScoped<IGameSpeciesLimits, GameSpeciesLimits>();

            services.AddScoped<ISaveFileRepository, SaveFileRepository>();

            services.AddScoped<ISaveEditRepository, SaveEditRepository>();

            services.AddScoped<IPlayerGameStatsRepository, PlayerGameStatsRepository>();

            services.AddScoped<IPlayerGameCounters, PlayerGameCountersReader>();

            services.AddScoped<IGameFileRepository, GameFileRepository>();

            services.AddScoped<IWebhookDeliveryRepository, WebhookDeliveryRepository>();

            services.ConfigureOptions<ConfigureDeliverWebhooksJob>();

            // A game that doesn't answer quickly is retried later rather than waited on.
            services.AddHttpClient(DeliverWebhooksJob.HttpClientName, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("PokeHub-Webhooks/1.0");
            });

            services.Configure<GamesKeyCloakOptions>(configuration.GetSection("Games:KeyCloak"));

            services.AddTransient<KeyCloakAuthDelegatingHandler>();

            services.AddHttpClient<IGameClientService, GameClientService>((serviceProvider, httpClient) =>
                {
                    GamesKeyCloakOptions keyCloakOptions = serviceProvider
                        .GetRequiredService<IOptions<GamesKeyCloakOptions>>().Value;

                    httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
                })
                .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

            // Sign-in by code for devices without a browser (the 3DS sync app). Players' own
            // tokens: no admin credentials on these calls.
            services.AddHttpClient<IDeviceSignInService, DeviceSignInService>((serviceProvider, httpClient) =>
            {
                GamesKeyCloakOptions keyCloakOptions = serviceProvider
                    .GetRequiredService<IOptions<GamesKeyCloakOptions>>().Value;

                httpClient.BaseAddress = new Uri(keyCloakOptions.OpenIdConnectUrl.TrimEnd('/') + "/");
                httpClient.Timeout = TimeSpan.FromSeconds(15);

                // Keycloak names its issuer after the host it's asked at, and only refreshes a
                // token at its issuer. The player approves at the public address, so the API
                // asks there too, over Docker's network.
                if (!string.IsNullOrEmpty(keyCloakOptions.PublicUrl))
                {
                    httpClient.DefaultRequestHeaders.Host = new Uri(keyCloakOptions.PublicUrl).Authority;
                }
            });

            // One instance: refreshed in the background, read by CORS, invalidated on registration.
            services.AddSingleton<GameOriginCache>();
            services.AddSingleton<IGameOriginCache>(sp => sp.GetRequiredService<GameOriginCache>());
            services.AddSingleton<IGameOriginPolicy>(sp => sp.GetRequiredService<GameOriginCache>());
            services.AddSingleton<IGameRateLimits>(sp => sp.GetRequiredService<GameOriginCache>());
            services.AddHostedService(sp => sp.GetRequiredService<GameOriginCache>());
        }

        private void AddDomainEventHandlers()
        {
            Type[] domainEventHandlers = Application.AssemblyReference.Assembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
                .ToArray();

            foreach (Type domainEventHandler in domainEventHandlers)
            {
                services.TryAddScoped(domainEventHandler);

                Type domainEvent = domainEventHandler
                    .GetInterfaces()
                    .Single(i => i.IsGenericType)
                    .GetGenericArguments()
                    .Single();

                Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

                services.Decorate(domainEventHandler, closedIdempotentHandler);
            }
        }


        private void AddIntegrationEventHandlers()
        {
            Type[] integrationEventHandlers = AssemblyReference.Assembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
                .ToArray();

            foreach (Type integrationEventHandler in integrationEventHandlers)
            {
                services.TryAddScoped(integrationEventHandler);

                Type integrationEvent = integrationEventHandler
                    .GetInterfaces()
                    .Single(i => i.IsGenericType)
                    .GetGenericArguments()
                    .Single();

                Type closedIdempotentHandler =
                    typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

                services.Decorate(integrationEventHandler, closedIdempotentHandler);
            }
        }
    }
}
