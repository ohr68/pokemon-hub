using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Infrastructure.Outbox;
using MassTransit;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;
using PokeHub.Modules.Pokedex.Application.Abstractions.Sync;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Species;
using PokeHub.Modules.Pokedex.Domain.Trainers;
using PokeHub.Modules.Pokedex.Infrastructure.Database;
using PokeHub.Modules.Pokedex.Infrastructure.Inbox;
using PokeHub.Modules.Pokedex.Infrastructure.Outbox;
using PokeHub.Modules.Pokedex.Infrastructure.PokeApi;
using PokeHub.Modules.Pokedex.Infrastructure.Pokemon;
using PokeHub.Modules.Pokedex.Infrastructure.Species;
using PokeHub.Modules.Pokedex.Infrastructure.SpeciesSync;
using PokeHub.Modules.Pokedex.Infrastructure.Trainers;
using PokeHub.Modules.Pokedex.Presentation;
using PokeHub.Modules.Users.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace PokeHub.Modules.Pokedex.Infrastructure;

public static class PokedexModule
{
    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        // Users publishes UserRegistered; the inbox turns each new user into a trainer.
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>();
    }

    extension(IServiceCollection services)
    {
        public void AddPokedexModule(IConfiguration configuration)
        {
            services.AddDomainEventHandlers();

            services.AddIntegrationEventHandlers();

            services.AddInfrastructure(configuration);
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddDbContext<PokedexDbContext>((sp, options) =>
                options.UseNpgsql(configuration.GetConnectionString("Database"),
                        npgsqlOptions => npgsqlOptions
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Pokedex))
                    .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PokedexDbContext>());

            services.Configure<OutboxOptions>(configuration.GetSection("Pokedex:Outbox"));

            services.ConfigureOptions<ConfigureProcessOutboxJob>();

            services.Configure<InboxOptions>(configuration.GetSection("Pokedex:Inbox"));

            services.ConfigureOptions<ConfigureProcessInboxJob>();

            services.AddScoped<ISpeciesRepository, SpeciesRepository>();
            services.AddScoped<ITrainerRepository, TrainerRepository>();
            services.AddScoped<IOwnedPokemonRepository, OwnedPokemonRepository>();

            services.Configure<PokeApiOptions>(configuration.GetSection("Pokedex:PokeApi"));

            services.AddHttpClient<IPokeApiClient, PokeApiClient>((serviceProvider, httpClient) =>
            {
                PokeApiOptions pokeApiOptions = serviceProvider.GetRequiredService<IOptions<PokeApiOptions>>().Value;

                httpClient.BaseAddress = pokeApiOptions.BaseUrl;
            });

            services.Configure<SpeciesSyncOptions>(configuration.GetSection("Pokedex:SpeciesSync"));

            services.ConfigureOptions<ConfigureSyncSpeciesJob>();

            services.AddScoped<ISpeciesSyncScheduler, SpeciesSyncScheduler>();
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
