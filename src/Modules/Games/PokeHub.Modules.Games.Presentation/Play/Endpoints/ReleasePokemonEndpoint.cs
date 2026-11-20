using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Presentation.Play.Contracts;
using PokeHub.Modules.Games.Presentation.Play.Docs;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed class ReleasePokemonEndpoint(ISender sender) : EndpointBase<PokemonRouteRequest>
{
    public override void Configure()
    {
        Post("save/pokemon/{pokemonId:guid}/release");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new ReleasePokemonEndpointSummary());
    }

    public override async Task HandleAsync(PokemonRouteRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new ReleasePokemonInGameCommand(User.GetClientId(), User.GetUserId(), req.PokemonId),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
