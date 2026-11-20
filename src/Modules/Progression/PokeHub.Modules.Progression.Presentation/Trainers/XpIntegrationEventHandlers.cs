using MediatR;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.IntegrationEvents;
using PokeHub.Modules.Progression.Application.Trainers;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Presentation.Trainers;

// Trainers earn XP for what happens to their Pokémon in the Pokédex (XpRules has the amounts).

internal sealed class PokemonObtainedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PokemonObtainedIntegrationEvent>
{
    public override async Task Handle(PokemonObtainedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        string source = $"pokemon-obtained:{e.PokemonId}";
        var awards = new List<XpAward>();

        switch (e.Origin)
        {
            case "starter":
                awards.Add(new XpAward(XpReason.ChoseStarter, source, e.OccurredOnUtc));
                break;
            case "game":
                awards.Add(new XpAward(XpReason.CaughtPokemon, source, e.OccurredOnUtc));
                if (e.IsShiny)
                {
                    awards.Add(new XpAward(XpReason.CaughtShiny, $"{source}:shiny", e.OccurredOnUtc));
                }

                if (e.IsNewSpecies)
                {
                    awards.Add(new XpAward(XpReason.NewSpecies, $"new-species:{e.SpeciesId}", e.OccurredOnUtc));
                }

                break;
            // Pokémon an admin grants earn nothing, so XP can't be handed out.
            default:
                return;
        }

        await AwardAsync(sender, e.TrainerId, awards, cancellationToken);
    }

    internal static async Task AwardAsync(ISender sender, Guid trainerId, IReadOnlyCollection<XpAward> awards, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new AwardXpCommand(trainerId, awards), cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(AwardXpCommand), result.Error);
        }
    }
}

internal sealed class PokemonTradedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PokemonTradedIntegrationEvent>
{
    // A trade publishes one event per Pokémon; each trainer earns once, for the one they receive.
    public override Task Handle(PokemonTradedIntegrationEvent e, CancellationToken cancellationToken = default) =>
        PokemonObtainedIntegrationEventHandler.AwardAsync(
            sender,
            e.ToTrainerId,
            [new XpAward(XpReason.Traded, $"trade:{e.TradeId}", e.OccurredOnUtc)],
            cancellationToken);
}

internal sealed class PokemonEvolvedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PokemonEvolvedIntegrationEvent>
{
    public override Task Handle(PokemonEvolvedIntegrationEvent e, CancellationToken cancellationToken = default) =>
        PokemonObtainedIntegrationEventHandler.AwardAsync(
            sender,
            e.TrainerId,
            [new XpAward(XpReason.Evolved, $"evolved:{e.PokemonId}:{e.ToSpeciesId}", e.OccurredOnUtc)],
            cancellationToken);
}
