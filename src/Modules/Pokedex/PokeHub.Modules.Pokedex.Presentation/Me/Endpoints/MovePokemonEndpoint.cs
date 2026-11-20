using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Pokemon.MovePokemon;
using PokeHub.Modules.Pokedex.Presentation.Me.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class MovePokemonEndpoint(ISender sender) : EndpointBase<MovePokemonRequest>
{
    public override void Configure()
    {
        Post("pokemon/{pokemonId:guid}/move");
        Policies(PokedexPermissions.UpdatePokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new MovePokemonEndpointSummary());
    }

    public override async Task HandleAsync(MovePokemonRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new MovePokemonCommand(User.GetUserId(), req.PokemonId, req.SaveId), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
