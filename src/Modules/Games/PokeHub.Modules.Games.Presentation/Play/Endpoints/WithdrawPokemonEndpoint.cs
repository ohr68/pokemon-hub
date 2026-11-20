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

internal sealed class WithdrawPokemonEndpoint(ISender sender) : EndpointBase<WithdrawPokemonRequest>
{
    public override void Configure()
    {
        Post("save/withdrawals");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<Guid>>(StatusCodes.Status200OK, "application/json"));
        Summary(new WithdrawPokemonEndpointSummary());
    }

    public override async Task HandleAsync(WithdrawPokemonRequest req, CancellationToken ct)
    {
        Result<IReadOnlyCollection<Guid>> result = await sender.Send(
            new WithdrawPokemonInGameCommand(
                User.GetClientId(),
                User.GetUserId(),
                (req.Withdrawals ?? [])
                    .Select(w => new WithdrawnPokemon(w.CatchKey, w.SpeciesId, w.Level, w.Nickname, w.DataFormat, w.Data))
                    .ToList()),
            ct);

        await SendResultAsync(result, ids => Results.Ok(ids));
    }
}
