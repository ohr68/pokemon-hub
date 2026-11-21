using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.IntegrationEvents;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed record ReportBadgesRequest(int TotalBadges, IReadOnlyCollection<ReportedBadge> Badges);

internal sealed class ReportBadgesEndpoint(ISender sender) : EndpointBase<ReportBadgesRequest>
{
    public override void Configure()
    {
        Put("save/badges");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status202Accepted));
        Summary(s =>
        {
            s.Summary = "Reports the badges the player has earned in the game";
            s.Description = "Send every badge earned so far (code and name) and how many the game has. New ones go on the trainer's page and earn XP and trophies, a few seconds later. Badges are never removed.";
        });
    }

    public override async Task HandleAsync(ReportBadgesRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new ReportBadgesInGameCommand(User.GetClientId(), User.GetUserId(), req.TotalBadges, req.Badges ?? []),
            ct);

        await SendResultAsync(result, () => Results.Accepted());
    }
}
