using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Species.GetGrowthData;
using PokeHub.Modules.Pokedex.Presentation.Species.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Endpoints;

internal sealed class GetGrowthDataEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("growth");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces<GrowthDataResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetGrowthDataEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<GrowthDataResponse> result = await sender.Send(new GetGrowthDataQuery(), ct);

        await SendResultAsync(result, data => Results.Ok(data));
    }
}
