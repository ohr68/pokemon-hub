using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Progression.Application.Trainers;

namespace PokeHub.Modules.Progression.Presentation.Trainers;

internal sealed class GetMyProgressEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("me");
        Policies(ProgressionPermissions.ReadProgression);
        Group<ProgressionEndpointGroup>();

        Description(builder => builder.Produces<TrainerProgressResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Gets the signed-in trainer's level and XP";
            s.Description = "Level, total XP, progress toward the next level and the latest XP awards. XP comes from choosing a starter, catching Pokémon in games (more for shinies and new species), trading and evolving.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<TrainerProgressResponse> result = await sender.Send(new GetTrainerProgressQuery(User.GetUserId()), ct);

        await SendResultAsync(result, progress => Results.Ok(progress));
    }
}

internal sealed class GetMyTrophiesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("me/trophies");
        Policies(ProgressionPermissions.ReadProgression);
        Group<ProgressionEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<TrophyResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Lists every trophy, earned or not";
            s.Description = "Each with its tier, the trainer's progress toward its target, and when it was earned.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<TrophyResponse>> result = await sender.Send(new GetTrainerTrophiesQuery(User.GetUserId()), ct);

        await SendResultAsync(result, trophies => Results.Ok(trophies));
    }
}

internal sealed class GetMyBadgesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("me/badges");
        Policies(ProgressionPermissions.ReadProgression);
        Group<ProgressionEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<GameBadgesResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Lists the trainer's badges per game";
            s.Description = "Each game that reported badges, how many it has in all, and the ones earned.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<GameBadgesResponse>> result = await sender.Send(new GetTrainerBadgesQuery(User.GetUserId()), ct);

        await SendResultAsync(result, games => Results.Ok(games));
    }
}
