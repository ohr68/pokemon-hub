using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class GetMeEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("/");
        Policies(PokedexPermissions.ReadPokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces<TrainerResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetMeEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<TrainerResponse> result = await sender.Send(new GetTrainerQuery(User.GetUserId()), ct);

        await SendResultAsync(result, trainer => Results.Ok(trainer));
    }
}
