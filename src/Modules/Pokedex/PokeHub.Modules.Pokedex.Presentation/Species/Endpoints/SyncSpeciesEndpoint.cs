using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Species.TriggerSpeciesSync;
using PokeHub.Modules.Pokedex.Presentation.Species.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Species.Endpoints;

internal sealed class SyncSpeciesEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Post("sync");
        Policies(PokedexPermissions.SyncSpecies);
        Group<SpeciesEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status202Accepted));
        Summary(new SyncSpeciesEndpointSummary());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result result = await sender.Send(new TriggerSpeciesSyncCommand(), ct);

        await SendResultAsync(result, () => Results.Accepted());
    }
}
