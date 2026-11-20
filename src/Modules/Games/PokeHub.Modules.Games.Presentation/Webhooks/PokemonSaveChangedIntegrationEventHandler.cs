using MediatR;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Webhooks;
using PokeHub.Modules.Pokedex.IntegrationEvents;

namespace PokeHub.Modules.Games.Presentation.Webhooks;

/// <summary>
/// A Pokémon moved into or out of a game's save: the game hears about it on its webhook as
/// pokemon.{change}.
/// </summary>
internal sealed class PokemonSaveChangedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PokemonSaveChangedIntegrationEvent>
{
    public override async Task Handle(
        PokemonSaveChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Result<Guid?> result = await sender.Send(
            new QueueWebhookEventCommand(
                integrationEvent.GameId,
                $"pokemon.{integrationEvent.Change}",
                new
                {
                    pokemonId = integrationEvent.PokemonId,
                    playerId = integrationEvent.TrainerId,
                    speciesId = integrationEvent.SpeciesId,
                    change = integrationEvent.Change,
                }),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(QueueWebhookEventCommand), result.Error);
        }
    }
}
