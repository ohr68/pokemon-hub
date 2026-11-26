using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Sprites;
using PokeHub.Modules.Pokedex.Presentation.Species;

namespace PokeHub.Modules.Pokedex.Presentation.Sprites;

internal sealed class GetPokemonSpriteEndpoint(ISender sender) : EndpointBase<GetPokemonSpriteRequest>
{
    public override void Configure()
    {
        Get("{speciesId:int}/sprite");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status200OK, contentType: "image/png"));
        Summary(s =>
        {
            s.Summary = "A species' small sprite (96×96 PNG)";
            s.Description = "PokéAPI's front sprite, fetched and kept by PokeHub, for devices on the local network that can't reach the internet themselves, such as the 3DS app. Pass shiny=true for the shiny one.";
        });
    }

    public override async Task HandleAsync(GetPokemonSpriteRequest req, CancellationToken ct)
    {
        Result<byte[]> result = await sender.Send(new GetPokemonSpriteQuery(req.SpeciesId, req.Shiny), ct);

        await SendResultAsync(result, png => Results.File(png, "image/png"));
    }
}

internal sealed class GetItemSpriteEndpoint(ISender sender) : EndpointBase<GetItemSpriteRequest>
{
    public override void Configure()
    {
        Get("items/{name}/sprite");
        AllowAnonymous();
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status200OK, contentType: "image/png"));
        Summary(s =>
        {
            s.Summary = "An item's sprite (30×30 PNG)";
            s.Description = "PokéAPI's item sprite, by its name there (poke-ball, rare-candy), fetched and kept by PokeHub for devices like the 3DS app.";
        });
    }

    public override async Task HandleAsync(GetItemSpriteRequest req, CancellationToken ct)
    {
        Result<byte[]> result = await sender.Send(new GetItemSpriteQuery(req.Name), ct);

        await SendResultAsync(result, png => Results.File(png, "image/png"));
    }
}

internal sealed class GetPokemonSpriteRequest
{
    public int SpeciesId { get; init; }

    [QueryParam]
    public bool Shiny { get; init; }
}

internal sealed class GetItemSpriteRequest
{
    public string Name { get; init; } = string.Empty;
}
