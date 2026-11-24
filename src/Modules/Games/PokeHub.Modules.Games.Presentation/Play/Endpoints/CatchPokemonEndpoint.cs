using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Presentation.Play.Contracts;
using PokeHub.Modules.Games.Presentation.Play.Docs;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed class CatchPokemonEndpoint(ISender sender) : EndpointBase<CatchPokemonRequest>
{
    public override void Configure()
    {
        Post("save/pokemon");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(new CatchPokemonEndpointSummary());
    }

    public override async Task HandleAsync(CatchPokemonRequest req, CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(
            new CatchPokemonInGameCommand(
                User.GetClientId(),
                User.GetUserId(),
                req.SpeciesId,
                req.Level,
                req.IsShiny,
                req.CatchKey,
                req.Nickname,
                req.FromOtherGame),
            ct);

        await SendResultAsync(result, pokemonId => Results.Ok(pokemonId));
    }
}
