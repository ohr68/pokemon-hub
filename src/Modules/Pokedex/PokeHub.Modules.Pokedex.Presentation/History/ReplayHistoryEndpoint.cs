using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Pokemon.ReplayHistory;

namespace PokeHub.Modules.Pokedex.Presentation.History;

internal sealed class HistoryEndpointGroup : Group
{
    public HistoryEndpointGroup()
    {
        Configure("pokedex/history", endpoint =>
        {
            endpoint.Description(builder => builder
                    .WithTags("Pokédex")
                    .ProducesProblem(StatusCodes.Status401Unauthorized)
                    .ProducesProblem(StatusCodes.Status403Forbidden)
                    .ProducesProblem(StatusCodes.Status500InternalServerError),
                clearDefaults: true);
        });
    }
}

internal sealed class ReplayHistoryEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Post("replay");
        // Admins only: the same permission as granting Pokémon.
        Policies(PokedexPermissions.GrantPokemon);
        Group<HistoryEndpointGroup>();

        Description(builder => builder.Produces<ReplayHistoryResponse>(StatusCodes.Status200OK, "application/json"));
        Summary(s =>
        {
            s.Summary = "Replays the ownership history as integration events";
            s.Description = "Publishes every Pokémon obtained, traded and evolved again, with their original dates, so a module added later (Progression's XP) can catch up. Safe to repeat: consumers handle each event and each award once.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<ReplayHistoryResponse> result = await sender.Send(new ReplayHistoryCommand(), ct);

        await SendResultAsync(result, replayed => Results.Ok(replayed));
    }
}
