using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Saves;
using PokeHub.Modules.Pokedex.Application.Saves.GetMySaves;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class GetMySavesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("saves");
        Policies(PokedexPermissions.ReadPokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<SaveResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetMySavesEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<SaveResponse>> result = await sender.Send(new GetMySavesQuery(User.GetUserId()), ct);

        await SendResultAsync(result, saves => Results.Ok(saves));
    }
}
