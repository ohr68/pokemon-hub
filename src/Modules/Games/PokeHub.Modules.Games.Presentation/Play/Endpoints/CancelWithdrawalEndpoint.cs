using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Play;

namespace PokeHub.Modules.Games.Presentation.Play.Endpoints;

internal sealed record CancelWithdrawalRequest(IReadOnlyCollection<string> CatchKeys, IReadOnlyCollection<string> PresentCatchKeys);

internal sealed class CancelWithdrawalEndpoint(ISender sender) : EndpointBase<CancelWithdrawalRequest>
{
    public override void Configure()
    {
        Post("save/withdrawals/cancel");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Puts withdrawn Pokémon back inside the game";
            s.Description = "For a withdrawal the game recorded but couldn't finish, so its save still holds the Pokémon. Send their catch keys and the catch keys of every Pokémon in the save now; one missing from the save is refused, so it can't be lost. All go back, or none. Repeating it changes nothing.";
        });
    }

    public override async Task HandleAsync(CancelWithdrawalRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new CancelWithdrawalInGameCommand(User.GetClientId(), User.GetUserId(), req.CatchKeys ?? [], req.PresentCatchKeys ?? []),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
