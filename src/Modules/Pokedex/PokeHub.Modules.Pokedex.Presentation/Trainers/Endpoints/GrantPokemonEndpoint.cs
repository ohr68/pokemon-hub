using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Pokemon.GrantPokemon;
using PokeHub.Modules.Pokedex.Presentation.Trainers.Contracts;
using PokeHub.Modules.Pokedex.Presentation.Trainers.Docs;

namespace PokeHub.Modules.Pokedex.Presentation.Trainers.Endpoints;

internal sealed class GrantPokemonEndpoint(ISender sender) : EndpointBase<GrantPokemonRequest>
{
    public override void Configure()
    {
        Post("{trainerId:guid}/pokemon");
        Policies(PokedexPermissions.GrantPokemon);
        Group<TrainersEndpointGroup>();

        Description(builder => builder.Produces<Guid>(StatusCodes.Status200OK, "application/json"));
        Summary(new GrantPokemonEndpointSummary());
    }

    public override async Task HandleAsync(GrantPokemonRequest req, CancellationToken ct)
    {
        Result<Guid> result = await sender.Send(
            new GrantPokemonCommand(req.TrainerId, req.SpeciesId, req.Level, req.IsShiny),
            ct);

        await SendResultAsync(result, pokemonId => Results.Ok(pokemonId));
    }
}
