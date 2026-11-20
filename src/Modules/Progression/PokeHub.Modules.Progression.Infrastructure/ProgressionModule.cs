using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Progression.Application.Abstractions.Data;
using PokeHub.Modules.Progression.Infrastructure.Database;
using PokeHub.Modules.Progression.Infrastructure.Inbox;
using PokeHub.Modules.Progression.Infrastructure.Outbox;
using PokeHub.Modules.Progression.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MassTransit;
using PokeHub.Modules.Pokedex.IntegrationEvents;
using PokeHub.Modules.Progression.Domain.Trainers;
using PokeHub.Modules.Progression.Infrastructure.Trainers;

namespace PokeHub.Modules.Progression.Infrastructure;

public static class ProgressionModule
{
    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        // What happens to trainers' Pokémon in the Pokédex earns them XP.
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<PokemonObtainedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<PokemonTradedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<PokemonEvolvedIntegrationEvent>>();
    }

    extension(IServiceCollection services)
    {
        public void AddProgressionModule(IConfiguration configuration)
        {
            services.AddDomainEventHandlers();

            services.AddIntegrationEventHandlers();

            services.AddInfrastructure(configuration);
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddDbContext<ProgressionDbContext>((sp, options) =>
                options.UseNpgsql(configuration.GetConnectionString("Database"),
                        npgsqlOptions => npgsqlOptions
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Progression))
                    .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProgressionDbContext>());

            services.Configure<OutboxOptions>(configuration.GetSection("Progression:Outbox"));

            services.ConfigureOptions<ConfigureProcessOutboxJob>();

            services.Configure<InboxOptions>(configuration.GetSection("Progression:Inbox"));

            services.ConfigureOptions<ConfigureProcessInboxJob>();

            services.AddScoped<ITrainerProgressRepository, TrainerProgressRepository>();
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
