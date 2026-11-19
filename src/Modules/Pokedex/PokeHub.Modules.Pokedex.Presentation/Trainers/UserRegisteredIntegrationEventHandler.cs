using MediatR;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Trainers.CreateTrainer;
using PokeHub.Modules.Users.IntegrationEvents;

namespace PokeHub.Modules.Pokedex.Presentation.Trainers;

/// <summary>
/// Every new PokeHub user becomes a trainer with the same identifier.
/// </summary>
internal sealed class UserRegisteredIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    public override async Task Handle(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new CreateTrainerCommand(integrationEvent.UserId, integrationEvent.FirstName),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(CreateTrainerCommand), result.Error);
        }
    }
}
