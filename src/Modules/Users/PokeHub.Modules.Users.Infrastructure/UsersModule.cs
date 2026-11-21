using PokeHub.Common.Application.Authorization;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Infrastructure.Outbox;
using PokeHub.Modules.Users.Application.Abstractions.Data;
using PokeHub.Modules.Users.Application.Abstractions.Identity;
using PokeHub.Modules.Users.Domain.Users;
using PokeHub.Modules.Users.Infrastructure.Authorization;
using PokeHub.Modules.Users.Infrastructure.Database;
using PokeHub.Modules.Users.Infrastructure.Identity;
using PokeHub.Modules.Users.Infrastructure.Inbox;
using PokeHub.Modules.Users.Infrastructure.Outbox;
using PokeHub.Modules.Users.Infrastructure.Users;
using PokeHub.Modules.Users.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace PokeHub.Modules.Users.Infrastructure;

public static class UsersModule
{
    extension(IServiceCollection services)
    {
        public void AddUsersModule(IConfiguration configuration)
        {
            services.AddDomainEventHandlers();

            services.AddIntegrationEventHandlers();

            services.AddInfrastructure(configuration);
        }

        private void AddInfrastructure(IConfiguration configuration)
        {
            services.AddScoped<IPermissionService, PermissionService>();

            services.Configure<KeyCloakOptions>(configuration.GetRequiredSection("Users:KeyCloak"));

            services.AddTransient<KeyCloakAuthDelegatingHandler>();

            services.AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
                {
                    KeyCloakOptions keyCloakOptions = serviceProvider
                        .GetRequiredService<IOptions<KeyCloakOptions>>().Value;

                    httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
                })
                .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

            services.AddTransient<IIdentityProviderService, IdentityProviderService>();

            services.AddDbContext<UsersDbContext>((sp, options) =>
                options.UseNpgsql(configuration.GetConnectionString("Database"),
                        npgsqlOptions => npgsqlOptions
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
                    .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddHostedService<FirstAdministratorService>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

            services.Configure<OutboxOptions>(configuration.GetSection("Users:Outbox"));

            services.ConfigureOptions<ConfigureProcessOutboxJob>();

            services.Configure<InboxOptions>(configuration.GetSection("Users:Inbox"));

            services.ConfigureOptions<ConfigureProcessInboxJob>();
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
