using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Presentation.Play.Contracts;
using PokeHub.Modules.Games.Presentation.Play.Docs;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed class DeliverPokemonEndpoint(ISender sender) : EndpointBase<DeliverPokemonRequest>
{
    public override void Configure()
    {
        Post("save/deliveries");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new DeliverPokemonEndpointSummary());
    }

    public override async Task HandleAsync(DeliverPokemonRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new DeliverPokemonInGameCommand(
                User.GetClientId(),
                User.GetUserId(),
                (req.Deliveries ?? []).Select(d => new DeliveredPokemon(d.PokemonId, d.CatchKey)).ToList()),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
