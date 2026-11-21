using MediatR;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Exceptions;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.Application.Wallets;
using PokeHub.Modules.Economy.Domain.Wallets;
using PokeHub.Modules.Pokedex.IntegrationEvents;
using PokeHub.Modules.Progression.IntegrationEvents;

namespace PokeHub.Modules.Economy.Presentation.Coins;

// What earns coins (CoinRules has the amounts). Each source pays once.

internal static class Coins
{
    internal static async Task EarnAsync(ISender sender, Guid trainerId, IReadOnlyCollection<CoinAward> awards, CancellationToken cancellationToken)
    {
        if (awards.Count == 0)
        {
            return;
        }

        Result result = await sender.Send(new EarnCoinsCommand(trainerId, awards), cancellationToken);

        if (result.IsFailure)
        {
            throw new PokeHubException(nameof(EarnCoinsCommand), result.Error);
        }
    }
}

internal sealed class PokemonObtainedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PokemonObtainedIntegrationEvent>
{
    // Only catches in games pay: Pokémon granted or bought don't.
    public override Task Handle(PokemonObtainedIntegrationEvent e, CancellationToken cancellationToken = default)
    {
        var awards = new List<CoinAward>();

        if (e.Origin == "game" && e.IsShiny)
        {
            awards.Add(new CoinAward(CoinReason.CaughtShiny, CoinRules.ShinyCatch, $"shiny:{e.PokemonId}", e.OccurredOnUtc));
        }

        if (e.Origin == "game" && e.IsRare)
        {
            awards.Add(new CoinAward(CoinReason.CaughtRare, CoinRules.RareCatch, $"rare:{e.PokemonId}", e.OccurredOnUtc));
        }

        return Coins.EarnAsync(sender, e.TrainerId, awards, cancellationToken);
    }
}

internal sealed class TrophyEarnedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<TrophyEarnedIntegrationEvent>
{
    public override Task Handle(TrophyEarnedIntegrationEvent e, CancellationToken cancellationToken = default) =>
        Coins.EarnAsync(
            sender,
            e.TrainerId,
            [new CoinAward(CoinReason.EarnedTrophy, CoinRules.Trophy(e.Tier), $"trophy:{e.TrophyCode}", e.OccurredOnUtc)],
            cancellationToken);
}

internal sealed class BadgeEarnedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<BadgeEarnedIntegrationEvent>
{
    public override Task Handle(BadgeEarnedIntegrationEvent e, CancellationToken cancellationToken = default) =>
        Coins.EarnAsync(
            sender,
            e.TrainerId,
            [new CoinAward(CoinReason.EarnedBadge, CoinRules.Badge, $"badge:{e.GameId}:{e.BadgeCode}", e.OccurredOnUtc)],
            cancellationToken);
}

internal sealed class TrainerLeveledUpIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<TrainerLeveledUpIntegrationEvent>
{
    // A jump of several levels pays for each level reached.
    public override Task Handle(TrainerLeveledUpIntegrationEvent e, CancellationToken cancellationToken = default) =>
        Coins.EarnAsync(
            sender,
            e.TrainerId,
            Enumerable.Range(e.FromLevel + 1, Math.Max(e.ToLevel - e.FromLevel, 0))
                .Select(level => new CoinAward(CoinReason.LeveledUp, CoinRules.Level(level), $"level:{level}", e.OccurredOnUtc))
                .ToList(),
            cancellationToken);
}
