using MediatR;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.IntegrationEvents;
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
        }

        // Granted and bought Pokémon earn no XP, but every species counts as discovered.
        if (awards.Count > 0)
        {
            await AwardAsync(sender, e.TrainerId, awards, cancellationToken);
        }

        await RefreshSpeciesAsync(sender, e.TrainerId, cancellationToken);
    }

    internal static async Task RefreshSpeciesAsync(ISender sender, Guid trainerId, CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new RefreshSpeciesDiscoveredCommand(trainerId), cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(RefreshSpeciesDiscoveredCommand), result.Error);
        }
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
    public override async Task Handle(PokemonTradedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await PokemonObtainedIntegrationEventHandler.AwardAsync(
            sender,
            e.ToTrainerId,
            [new XpAward(XpReason.Traded, $"trade:{e.TradeId}", e.OccurredOnUtc)],
            cancellationToken);

        await PokemonObtainedIntegrationEventHandler.RefreshSpeciesAsync(sender, e.ToTrainerId, cancellationToken);
    }
}

internal sealed class PokemonEvolvedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PokemonEvolvedIntegrationEvent>
{
    public override async Task Handle(PokemonEvolvedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        await PokemonObtainedIntegrationEventHandler.AwardAsync(
            sender,
            e.TrainerId,
            [new XpAward(XpReason.Evolved, $"evolved:{e.PokemonId}:{e.ToSpeciesId}", e.OccurredOnUtc)],
            cancellationToken);

        await PokemonObtainedIntegrationEventHandler.RefreshSpeciesAsync(sender, e.TrainerId, cancellationToken);
    }
}

internal sealed class GameBadgesReportedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<GameBadgesReportedIntegrationEvent>
{
    public override async Task Handle(GameBadgesReportedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(
            new RecordBadgesCommand(
                e.TrainerId,
                e.GameId,
                e.GameName,
                e.TotalBadges,
                e.Badges.Select(b => new BadgeInfo(b.Code, b.Name)).ToList()),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(RecordBadgesCommand), result.Error);
        }
    }
}
