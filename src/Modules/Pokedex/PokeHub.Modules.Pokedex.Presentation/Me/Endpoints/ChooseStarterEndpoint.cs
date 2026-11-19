using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Trainers.ChooseStarter;
using PokeHub.Modules.Pokedex.Presentation.Me.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Me.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class ChooseStarterEndpoint(ISender sender) : EndpointBase<ChooseStarterRequest>
{
    public override void Configure()
    {
        Post("starter");
        Policies(PokedexPermissions.UpdatePokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(new ChooseStarterEndpointSummary());
    }

    public override async Task HandleAsync(ChooseStarterRequest req, CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(new ChooseStarterCommand(User.GetUserId(), req.SpeciesId), ct);

        await SendResultAsync(result, pokemonId => Results.Ok(pokemonId));
    }
}
