using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Games.GetGames;
using PokeHub.Modules.Games.Presentation.Games.Docs;

namespace PokeHub.Modules.Games.Presentation.Games.Endpoints;

internal sealed class GetGamesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("/");
        AllowAnonymous();
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<GameResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetGamesEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<GameResponse>> result = await sender.Send(new GetGamesQuery(), ct);

        await SendResultAsync(result, games => Results.Ok(games));
    }
}
