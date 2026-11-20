using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.IntegrationEvents;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.Events;

// Tell the other modules (Progression awards XP for them) what happened to trainers' Pokémon.

internal sealed class PokemonObtainedDomainEventHandler(IDbConnectionFactory dbConnectionFactory, IEventBus eventBus)
    : DomainEventHandler<PokemonObtainedDomainEvent>
{
    public override async Task Handle(PokemonObtainedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // New to the trainer: no other Pokémon of theirs was ever this species.
        bool hadSpecies = await connection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1 FROM pokedex.ownership_history
                WHERE trainer_id = @TrainerId AND species_id = @SpeciesId AND pokemon_id <> @PokemonId)
            """,
            new { domainEvent.TrainerId, domainEvent.SpeciesId, domainEvent.PokemonId });

        await eventBus.PublishAsync(
            new PokemonObtainedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.PokemonId,
                domainEvent.TrainerId,
                domainEvent.SpeciesId,
                domainEvent.Origin.ToString().ToLowerInvariant(),
                domainEvent.IsShiny,
                !hadSpecies),
            cancellationToken);
    }
}

internal sealed class PokemonTradedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<PokemonTradedDomainEvent>
{
    public override Task Handle(PokemonTradedDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        eventBus.PublishAsync(
            new PokemonTradedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.PokemonId,
                domainEvent.TradeId,
                domainEvent.FromTrainerId,
                domainEvent.ToTrainerId),
            cancellationToken);
}

internal sealed class PokemonEvolvedDomainEventHandler(IEventBus eventBus) : DomainEventHandler<PokemonEvolvedDomainEvent>
{
    public override Task Handle(PokemonEvolvedDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        eventBus.PublishAsync(
            new PokemonEvolvedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.PokemonId,
                domainEvent.TrainerId,
                domainEvent.FromSpeciesId,
                domainEvent.ToSpeciesId),
            cancellationToken);
}
