using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Pokedex.Application.Trainers.SetAvatar;
using PokeHub.Modules.Pokedex.Presentation.Me.Contracts;

namespace PokeHub.Modules.Pokedex.Presentation.Me.Endpoints;

internal sealed class SetAvatarEndpoint(ISender sender) : EndpointBase<SetAvatarRequest>
{
    public override void Configure()
    {
        Put("avatar");
        Policies(PokedexPermissions.UpdatePokedex);
        Group<MeEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Sets the trainer's picture";
            s.Description = "Shows one of the trainer's own Pokémon as their picture, following it if it evolves. Send an empty pokemonId to remove it.";
        });
    }

    public override async Task HandleAsync(SetAvatarRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new SetAvatarCommand(User.GetUserId(), req.PokemonId), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}
