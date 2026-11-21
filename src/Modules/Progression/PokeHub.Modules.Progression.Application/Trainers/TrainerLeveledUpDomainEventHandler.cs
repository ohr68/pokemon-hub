using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Modules.Progression.Domain.Trainers;
using PokeHub.Modules.Progression.IntegrationEvents;

namespace PokeHub.Modules.Progression.Application.Trainers;

internal sealed class TrainerLeveledUpDomainEventHandler(IEventBus eventBus) : DomainEventHandler<TrainerLeveledUpDomainEvent>
{
    public override Task Handle(TrainerLeveledUpDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        eventBus.PublishAsync(
            new TrainerLeveledUpIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.TrainerId,
                domainEvent.FromLevel,
                domainEvent.ToLevel),
            cancellationToken);
}

internal sealed class TrophyEarnedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<TrophyEarnedDomainEvent>
{
    public override Task Handle(TrophyEarnedDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        eventBus.PublishAsync(
            new TrophyEarnedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.TrainerId,
                domainEvent.TrophyCode,
                domainEvent.Tier.ToString().ToLowerInvariant()),
            cancellationToken);
}
