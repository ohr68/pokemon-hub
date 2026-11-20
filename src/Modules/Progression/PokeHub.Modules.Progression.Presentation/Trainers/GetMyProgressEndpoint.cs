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
