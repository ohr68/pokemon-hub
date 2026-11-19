using MediatR;

namespace PokeHub.Common.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }
}
