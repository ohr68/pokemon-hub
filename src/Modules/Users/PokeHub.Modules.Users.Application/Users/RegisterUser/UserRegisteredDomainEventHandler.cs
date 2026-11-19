using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Users.Application.Users.GetUser;
using PokeHub.Modules.Users.Domain.Users;
using PokeHub.Modules.Users.IntegrationEvents;
using MediatR;

namespace PokeHub.Modules.Users.Application.Users.RegisterUser;

internal sealed class UserRegisteredDomainEventHandler(
    ISender sender,
    IEventBus eventBus)
    : DomainEventHandler<UserRegisteredDomainEvent>
{
    public override async Task Handle(
        UserRegisteredDomainEvent notification,
        CancellationToken cancellationToken = default)
    {
        Result<UserResponse> result = await sender.Send(new GetUserQuery(notification.UserId), cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(GetUserQuery), result.Error);
        }

        await eventBus.PublishAsync(
            new UserRegisteredIntegrationEvent(
                notification.Id,
                notification.OccurredOnUtc,
                result.Value.Id,
                result.Value.Email,
                result.Value.FirstName,
                result.Value.LastName),
            cancellationToken);
    }
}
