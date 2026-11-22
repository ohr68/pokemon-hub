using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetMyPokemonDetails;
using PokeHub.Modules.Pokedex.Presentation.Me.Contracts;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class GetMyPokemonDetailsEndpoint(ISender sender) : EndpointBase<PokemonRouteRequest>
{
    public override void Configure()
    {
        Get("pokemon/{pokemonId:guid}");
        Policies(PokedexPermissions.ReadPokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces<MyPokemonDetailsResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "One of the trainer's Pokémon, with how it is in each game");
    }

    public override async Task HandleAsync(PokemonRouteRequest req, CancellationToken ct)
    {
        Result<MyPokemonDetailsResponse> result = await sender.Send(new GetMyPokemonDetailsQuery(User.GetUserId(), req.PokemonId), ct);

        await SendResultAsync(result, details => Results.Ok(details));
    }
}
