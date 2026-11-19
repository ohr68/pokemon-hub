using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Pokemon.GetTrainerPokemon;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class GetMyPokemonEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("pokemon");
        Policies(PokedexPermissions.ReadPokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<PokemonResponse>>(
            StatusCodes.Status200OK,
            "application/json"));
        Summary(new GetMyPokemonEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<PokemonResponse>> result = await sender.Send(
            new GetTrainerPokemonQuery(User.GetUserId()),
            ct);

        await SendResultAsync(result, pokemon => Results.Ok(pokemon));
    }
}
