using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Species.GetGen3Data;
using PokeHub.Modules.Pokedex.Presentation.Species.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Endpoints;

internal sealed class GetGen3DataEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("gen3");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces<Gen3DataResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetGen3DataEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<Gen3DataResponse> result = await sender.Send(new GetGen3DataQuery(), ct);

        await SendResultAsync(result, data => Results.Ok(data));
    }
}
