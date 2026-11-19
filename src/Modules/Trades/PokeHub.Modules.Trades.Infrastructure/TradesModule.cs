using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Trades.Application.Abstractions.Data;
using PokeHub.Modules.Trades.Domain.Trades;
using PokeHub.Modules.Trades.Infrastructure.Database;
using PokeHub.Modules.Trades.Infrastructure.Trades;
using PokeHub.Modules.Trades.Infrastructure.Inbox;
using PokeHub.Modules.Trades.Infrastructure.Outbox;
using PokeHub.Modules.Trades.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PokeHub.Modules.Trades.Infrastructure;

public static class TradesModule
{
    extension(IServiceCollection services)
    {
        public void AddTradesModule(IConfiguration configuration)
        {
            services.AddDomainEventHandlers();

            services.AddIntegrationEventHandlers();

            services.AddInfrastructure(configuration);
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddDbContext<TradesDbContext>((sp, options) =>
                options.UseNpgsql(configuration.GetConnectionString("Database"),
                        npgsqlOptions => npgsqlOptions
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Trades))
                    .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TradesDbContext>());

            services.Configure<OutboxOptions>(configuration.GetSection("Trades:Outbox"));

            services.ConfigureOptions<ConfigureProcessOutboxJob>();

            services.Configure<InboxOptions>(configuration.GetSection("Trades:Inbox"));

            services.ConfigureOptions<ConfigureProcessInboxJob>();

            services.AddScoped<ITradeRepository, TradeRepository>();
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
