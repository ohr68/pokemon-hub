using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Pokemon.ReleasePokemon;
using PokeHub.Modules.Pokedex.Presentation.Me.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class ReleasePokemonEndpoint(ISender sender) : EndpointBase<PokemonRouteRequest>
{
    public override void Configure()
    {
        Post("pokemon/{pokemonId:guid}/release");
        Policies(PokedexPermissions.UpdatePokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new ReleasePokemonEndpointSummary());
    }

    public override async Task HandleAsync(PokemonRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new ReleasePokemonCommand(User.GetUserId(), req.PokemonId), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
