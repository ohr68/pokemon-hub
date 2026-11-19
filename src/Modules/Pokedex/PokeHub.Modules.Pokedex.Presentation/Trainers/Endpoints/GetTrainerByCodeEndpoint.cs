using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Trainers.GetTrainerByCode;
using PokeHub.Modules.Pokedex.Presentation.Trainers.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Trainers.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Trainers.Endpoints;

internal sealed class GetTrainerByCodeEndpoint(ISender sender) : EndpointBase<TrainerCodeRequest>
{
    public override void Configure()
    {
        Get("by-code/{code}");
        Policies(PokedexPermissions.ReadPokedex);
        Group<TrainersEndpointGroup>();

        Description(builder => builder.Produces<TrainerShowcaseResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetTrainerByCodeEndpointSummary());
    }

    public override async Task HandleAsync(TrainerCodeRequest req, CancellationToken ct)
    {
        Result<TrainerShowcaseResponse> result = await sender.Send(new GetTrainerByCodeQuery(req.Code), ct);

        await SendResultAsync(result, trainer => Results.Ok(trainer));
    }
}
