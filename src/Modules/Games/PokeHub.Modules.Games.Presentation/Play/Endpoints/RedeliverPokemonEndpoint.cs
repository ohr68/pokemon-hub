using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed record RedeliverPokemonRequest(IReadOnlyCollection<Guid> PokemonIds, IReadOnlyCollection<string> PresentCatchKeys);

internal sealed class RedeliverPokemonEndpoint(ISender sender) : EndpointBase<RedeliverPokemonRequest>
{
    public override void Configure()
    {
        Post("save/redeliveries");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Queues Pokémon missing from the game's save for delivery again";
            s.Description = "For Pokémon PokeHub has inside your game that its save no longer holds, such as after restoring an older save. Send the catch keys of every Pokémon in the save now; a Pokémon still in the save is refused, so it can't be duplicated.";
        });
    }

    public override async Task HandleAsync(RedeliverPokemonRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new RedeliverPokemonInGameCommand(User.GetClientId(), User.GetUserId(), req.PokemonIds, req.PresentCatchKeys),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
