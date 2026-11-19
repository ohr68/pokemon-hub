using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Species.GetSpecies;
using PokeHub.Modules.Pokedex.Application.Species.GetSpeciesById;
using PokeHub.Modules.Pokedex.Presentation.Species.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Species.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Endpoints;

internal sealed class GetSpeciesByIdEndpoint(ISender sender) : EndpointBase<GetSpeciesByIdRequest>
{
    public override void Configure()
    {
        Get("{id:int}");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces<SpeciesResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetSpeciesByIdEndpointSummary());
    }

    public override async Task HandleAsync(GetSpeciesByIdRequest req, CancellationToken ct)
    {
        Result<SpeciesResponse> result = await sender.Send(new GetSpeciesByIdQuery(req.Id), ct);

        await SendResultAsync(result, species => Results.Ok(species));
    }
}
