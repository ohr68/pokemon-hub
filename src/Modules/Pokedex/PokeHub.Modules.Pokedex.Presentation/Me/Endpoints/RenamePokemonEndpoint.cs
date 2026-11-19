using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Pokemon.RenamePokemon;
using PokeHub.Modules.Pokedex.Presentation.Me.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class RenamePokemonEndpoint(ISender sender) : EndpointBase<RenamePokemonRequest>
{
    public override void Configure()
    {
        Put("pokemon/{pokemonId:guid}/nickname");
        Policies(PokedexPermissions.UpdatePokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(new RenamePokemonEndpointSummary());
    }

    public override async Task HandleAsync(RenamePokemonRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new RenamePokemonCommand(User.GetUserId(), req.PokemonId, req.Nickname),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
