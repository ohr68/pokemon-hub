using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Progress.GetPokedexProgress;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class GetMyProgressEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("progress");
        Policies(PokedexPermissions.ReadPokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces<PokedexProgressResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(new GetMyProgressEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<PokedexProgressResponse> result = await sender.Send(new GetPokedexProgressQuery(User.GetUserId()), ct);

        await SendResultAsync(result, progress => Results.Ok(progress));
    }
}
