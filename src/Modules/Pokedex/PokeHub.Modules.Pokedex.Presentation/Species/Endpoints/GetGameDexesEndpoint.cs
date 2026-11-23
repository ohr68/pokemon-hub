using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Species.GetGameDexes;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Endpoints;

internal sealed class GetGameDexesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("game-dexes");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<GameDexResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "Lists the main-series games the Pokédex can be filtered by");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<GameDexResponse>> result = await sender.Send(new GetGameDexesQuery(), ct);

        await SendResultAsync(result, dexes => Results.Ok(dexes));
    }
}
