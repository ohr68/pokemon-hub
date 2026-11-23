using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Species.GetGameDexes;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Endpoints;

internal sealed class GetGameDexSpeciesRequest
{
    public string Slug { get; init; } = string.Empty;
}

internal sealed class GetGameDexSpeciesEndpoint(ISender sender) : EndpointBase<GetGameDexSpeciesRequest>
{
    public override void Configure()
    {
        Get("game-dexes/{slug}/species");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<int>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "The National Dex numbers in a game's own Pokédex, in the game's order");
    }

    public override async Task HandleAsync(GetGameDexSpeciesRequest req, CancellationToken ct)
    {
        Result<IReadOnlyCollection<int>> result = await sender.Send(new GetGameDexSpeciesQuery(req.Slug), ct);

        await SendResultAsync(result, species => Results.Ok(species));
    }
}
