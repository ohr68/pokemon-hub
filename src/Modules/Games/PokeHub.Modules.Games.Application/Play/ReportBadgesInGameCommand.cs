using FluentValidation;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.IntegrationEvents;

namespace PokeHub.Modules.Games.Application.Play;

/// <param name="TotalBadges">How many badges the game has in all.</param>
/// <param name="Badges">Every badge the player has earned so far; repeating them is fine.</param>
public sealed record ReportBadgesInGameCommand(
    string? ClientId,
    Guid TrainerId,
    int TotalBadges,
    IReadOnlyCollection<ReportedBadge> Badges) : ICommand;

internal sealed class ReportBadgesInGameCommandValidator : AbstractValidator<ReportBadgesInGameCommand>
{
    public ReportBadgesInGameCommandValidator()
    {
        RuleFor(c => c.TotalBadges).InclusiveBetween(1, 100);
        RuleFor(c => c.Badges).Must((c, badges) => badges.Count <= c.TotalBadges)
            .WithMessage("More badges than the game has.");
        RuleForEach(c => c.Badges).ChildRules(badge =>
        {
            badge.RuleFor(b => b.Code).NotEmpty().MaximumLength(50);
            badge.RuleFor(b => b.Name).NotEmpty().MaximumLength(100);
        });
        RuleFor(c => c.Badges).Must(b => b.Select(x => x.Code).Distinct(StringComparer.Ordinal).Count() == b.Count)
            .WithMessage("Each badge can only appear once.");
    }
}

internal sealed class ReportBadgesInGameCommandHandler(
    IGameRepository gameRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ReportBadgesInGameCommand>
{
    public async Task<Result> Handle(ReportBadgesInGameCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure(game.Error);
        }

        // Nothing to store here: badges belong to the trainer's progression.
        await eventBus.PublishAsync(
            new GameBadgesReportedIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                request.TrainerId,
                game.Value.Id,
                game.Value.Name,
                request.TotalBadges,
                request.Badges),
            cancellationToken);

        return Result.Success();
    }
}
