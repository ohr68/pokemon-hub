using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Economy.Application.Abstractions.Data;
using PokeHub.Modules.Economy.Infrastructure.Database;
using PokeHub.Modules.Economy.Infrastructure.Inbox;
using PokeHub.Modules.Economy.Infrastructure.Outbox;
using PokeHub.Modules.Economy.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MassTransit;
using PokeHub.Modules.Economy.Domain.Shop;
using PokeHub.Modules.Economy.Domain.Wallets;
using PokeHub.Modules.Economy.Infrastructure.Shop;
using PokeHub.Modules.Economy.Infrastructure.Wallets;
using PokeHub.Modules.Pokedex.IntegrationEvents;
using PokeHub.Modules.Progression.IntegrationEvents;

using PokeHub.Modules.Economy.Infrastructure.PublicApi;
using PokeHub.Modules.Economy.PublicApi;

namespace PokeHub.Modules.Economy.Infrastructure;

public static class EconomyModule
{
    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
    {
        // Rare and shiny catches, and trainers' trophies, badges and levels, earn coins.
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<PokemonObtainedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<TrophyEarnedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<BadgeEarnedIntegrationEvent>>();
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<TrainerLeveledUpIntegrationEvent>>();
    }

    extension(IServiceCollection services)
    {
        public void AddEconomyModule(IConfiguration configuration)
        {
            services.AddDomainEventHandlers();

            services.AddIntegrationEventHandlers();

            services.AddInfrastructure(configuration);
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddDbContext<EconomyDbContext>((sp, options) =>
                options.UseNpgsql(configuration.GetConnectionString("Database"),
                        npgsqlOptions => npgsqlOptions
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Economy))
                    .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EconomyDbContext>());

            services.Configure<OutboxOptions>(configuration.GetSection("Economy:Outbox"));

            services.ConfigureOptions<ConfigureProcessOutboxJob>();

            services.Configure<InboxOptions>(configuration.GetSection("Economy:Inbox"));

            services.ConfigureOptions<ConfigureProcessInboxJob>();

            services.AddScoped<IWalletRepository, WalletRepository>();

            services.AddScoped<IShopRepository, ShopRepository>();

            services.AddScoped<IEconomyApi, EconomyApi>();
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
