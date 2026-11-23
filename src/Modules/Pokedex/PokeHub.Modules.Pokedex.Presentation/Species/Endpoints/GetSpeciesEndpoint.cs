using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Species.GetSpecies;
using PokeHub.Modules.Pokedex.Presentation.Species.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Species.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Endpoints;

internal sealed class GetSpeciesEndpoint(ISender sender) : EndpointBase<GetSpeciesRequest>
{
    public override void Configure()
    {
        Get("/");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces<SpeciesPageResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetSpeciesEndpointSummary());
    }

    public override async Task HandleAsync(GetSpeciesRequest req, CancellationToken ct)
    {
        Result<SpeciesPageResponse> result = await sender.Send(
            new GetSpeciesQuery(req.Search, req.Type, req.Generation, req.Page, req.PageSize, req.Game, req.National),
            ct);

        await SendResultAsync(result, page => Results.Ok(page));
    }
}
