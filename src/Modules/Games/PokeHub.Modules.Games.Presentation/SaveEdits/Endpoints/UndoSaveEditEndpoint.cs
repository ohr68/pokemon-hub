using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.SaveEdits.UndoSaveEdit;
using PokeHub.Modules.Games.Presentation.Play;

namespace PokeHub.Modules.Games.Presentation.SaveEdits.Endpoints;

internal sealed class UndoSaveEditEndpoint(ISender sender) : EndpointBase<UndoSaveEditRequest>
{
    public override void Configure()
    {
        Post("save/edits/{editId:guid}/unwritten");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Undoes an edit the device couldn't write into the game";
            s.Description = "Call it when writing the save from save/edits failed and the game kept its old save. The delivered Pokémon and shop items wait again, and the withdrawn Pokémon are back inside the game. Without this call, the next save sent settles it the same way.";
        });
    }

    public override async Task HandleAsync(UndoSaveEditRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new UndoSaveEditCommand(User.GetClientId(), User.GetUserId(), req.EditId), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class UndoSaveEditRequest
{
    [BindFrom("editId")]
    public Guid EditId { get; init; }
}
