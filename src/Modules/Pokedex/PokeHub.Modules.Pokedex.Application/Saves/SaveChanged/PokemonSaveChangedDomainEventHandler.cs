using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;
using PokeHub.Modules.Pokedex.IntegrationEvents;

namespace PokeHub.Modules.Pokedex.Application.Saves.SaveChanged;

/// <summary>
/// Tells the other modules (the Games module sends it to the game's webhook) which game's
/// save a Pokémon moved into or out of.
/// </summary>
internal sealed class PokemonSaveChangedDomainEventHandler(ISaveRepository saveRepository, IEventBus eventBus)
    : DomainEventHandler<PokemonSaveChangedDomainEvent>
{
    public override async Task Handle(PokemonSaveChangedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Save? save = await saveRepository.GetAsync(domainEvent.SaveId, cancellationToken);

        if (save is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new PokemonSaveChangedIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.PokemonId,
                domainEvent.TrainerId,
                domainEvent.SpeciesId,
                save.GameId,
                ToSnakeCase(domainEvent.Change)),
            cancellationToken);
    }

    private static string ToSnakeCase(SaveChange change) => change switch
    {
        SaveChange.MovedIn => "moved_in",
        SaveChange.MovedOut => "moved_out",
        SaveChange.Delivered => "delivered",
        SaveChange.Withdrawn => "withdrawn",
        SaveChange.Released => "released",
        SaveChange.LeftGame => "left_game",
        _ => change.ToString().ToLowerInvariant(),
    };
}
