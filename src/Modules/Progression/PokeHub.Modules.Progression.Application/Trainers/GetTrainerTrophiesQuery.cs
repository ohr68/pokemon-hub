using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <summary>Every trophy, earned or not, with how close the trainer is.</summary>
public sealed record GetTrainerTrophiesQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<TrophyResponse>>;

/// <param name="Tier">bronze, silver or gold.</param>
/// <param name="Progress">The trainer's count toward <paramref name="Target"/>, capped at it.</param>
public sealed record TrophyResponse(
    string Code,
    string Name,
    string Description,
    string Tier,
    int Progress,
    int Target,
    DateTime? EarnedOnUtc);

internal sealed class GetTrainerTrophiesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTrainerTrophiesQuery, IReadOnlyCollection<TrophyResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrophyResponse>>> Handle(GetTrainerTrophiesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT starters_chosen AS StartersChosen, catches AS Catches, shiny_catches AS ShinyCatches,
                   species_discovered AS SpeciesDiscovered, trades AS Trades, evolutions AS Evolutions, level AS Level
            FROM progression.trainer_progress WHERE trainer_id = @TrainerId;

            SELECT trophy_code AS TrophyCode, earned_on_utc AS EarnedOnUtc
            FROM progression.trainer_trophies WHERE trainer_id = @TrainerId;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.TrainerId });

        StatsRow stats = await results.ReadSingleOrDefaultAsync<StatsRow>() ?? new StatsRow(0, 0, 0, 0, 0, 0, 1);
        var earned = (await results.ReadAsync<EarnedRow>()).ToDictionary(e => e.TrophyCode, e => e.EarnedOnUtc);

        return Trophies.All
            .Select(t => new TrophyResponse(
                t.Code,
                t.Name,
                t.Description,
                t.Tier.ToString().ToLowerInvariant(),
                Math.Min(stats.Value(t.Stat), t.Target),
                t.Target,
                earned.TryGetValue(t.Code, out DateTime on) ? on : null))
            .ToList();
    }

    private sealed record StatsRow(int StartersChosen, int Catches, int ShinyCatches, int SpeciesDiscovered, int Trades, int Evolutions, int Level)
    {
        public int Value(TrainerStat stat) => stat switch
        {
            TrainerStat.StartersChosen => StartersChosen,
            TrainerStat.Catches => Catches,
            TrainerStat.ShinyCatches => ShinyCatches,
            TrainerStat.SpeciesDiscovered => SpeciesDiscovered,
            TrainerStat.Trades => Trades,
            TrainerStat.Evolutions => Evolutions,
            TrainerStat.Level => Level,
            _ => 0,
        };
    }

    private sealed record EarnedRow(string TrophyCode, DateTime EarnedOnUtc);
}
