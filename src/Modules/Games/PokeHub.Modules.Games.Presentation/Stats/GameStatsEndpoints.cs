using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Infrastructure.Authentication;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Stats;
using PokeHub.Modules.Games.Presentation.Games;
using PokeHub.Modules.Games.Presentation.Play;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Presentation.Stats;

internal sealed record ReportGameStatsRequest(
    long PlayTimeSeconds,
    long Money,
    int PokedexSeen,
    int PokedexOwned,
    IReadOnlyDictionary<string, long> Counters);

internal sealed record ReportBattleDataRequest(IReadOnlyCollection<PokemonBattleReport> Pokemon);

internal sealed class ReportGameStatsEndpoint(ISender sender) : EndpointBase<ReportGameStatsRequest>
{
    public override void Configure()
    {
        Put("save/stats");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Reports the player's numbers in this game";
            s.Description = "Play time, money, Pokédex and the game's own counters by name. Each report replaces the last.";
        });
    }

    public override async Task HandleAsync(ReportGameStatsRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(
            new ReportGameStatsCommand(User.GetClientId(), User.GetUserId(), req.PlayTimeSeconds, req.Money, req.PokedexSeen, req.PokedexOwned, req.Counters),
            ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class ReportBattleDataEndpoint(ISender sender) : EndpointBase<ReportBattleDataRequest>
{
    public override void Configure()
    {
        Put("save/battle-data");
        Policies(GamesPermissions.UpdatePokedex);
        Group<PlayEndpointGroup>();

        Description(builder => builder.Produces(StatusCodes.Status204NoContent));
        Summary(s =>
        {
            s.Summary = "Reports battle data for the Pokémon in the game";
            s.Description = "Moves, nature, IVs, EVs and friendship by catch key, plus current stats for the party. Unknown catch keys are skipped.";
        });
    }

    public override async Task HandleAsync(ReportBattleDataRequest req, CancellationToken ct)
    {
        Result result = await sender.Send(new ReportBattleDataCommand(User.GetClientId(), User.GetUserId(), req.Pokemon), ct);

        await SendResultAsync(result, Results.NoContent);
    }
}

internal sealed class GetMyGameStatsEndpoint(ISender sender) : EndpointWithoutRequestBase
{
    public override void Configure()
    {
        Get("me/stats");
        Policies(GamesPermissions.ReadPokedex);
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<IReadOnlyCollection<GameStatsResponse>>(StatusCodes.Status200OK, "application/json"));
        Summary(s => s.Summary = "The trainer's numbers in each game");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IReadOnlyCollection<GameStatsResponse>> result = await sender.Send(new GetMyGameStatsQuery(User.GetUserId()), ct);

        await SendResultAsync(result, stats => Results.Ok(stats));
    }
}
