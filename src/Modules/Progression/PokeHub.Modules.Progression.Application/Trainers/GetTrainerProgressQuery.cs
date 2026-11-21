using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

public sealed record GetTrainerProgressQuery(Guid TrainerId) : IQuery<TrainerProgressResponse>;

/// <param name="XpIntoLevel">XP earned since reaching the current level.</param>
/// <param name="XpForNextLevel">XP the current level takes in all; empty at the top level.</param>
/// <param name="RecentXp">The latest awards, newest first.</param>
public sealed record TrainerProgressResponse(
    int Level,
    long TotalXp,
    long XpIntoLevel,
    long? XpForNextLevel,
    int MaxLevel,
    TrainerStatsResponse Stats,
    IReadOnlyCollection<XpEntryResponse> RecentXp);

public sealed record TrainerStatsResponse(int Catches, int ShinyCatches, int SpeciesDiscovered, int Trades, int Evolutions, int Badges, int Trophies);

/// <param name="Reason">chose_starter, caught_pokemon, caught_shiny, new_species, traded or evolved.</param>
public sealed record XpEntryResponse(string Reason, int Amount, DateTime EarnedOnUtc);

internal sealed class GetTrainerProgressQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTrainerProgressQuery, TrainerProgressResponse>
{
    private const int RecentCount = 10;

    public async Task<Result<TrainerProgressResponse>> Handle(GetTrainerProgressQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT total_xp AS TotalXp, catches AS Catches, shiny_catches AS ShinyCatches,
                   species_discovered AS SpeciesDiscovered, trades AS Trades, evolutions AS Evolutions, badges AS Badges,
                   (SELECT count(*)::int FROM progression.trainer_trophies t WHERE t.trainer_id = @TrainerId) AS Trophies
            FROM progression.trainer_progress WHERE trainer_id = @TrainerId;

            SELECT reason AS Reason, amount AS Amount, earned_on_utc AS EarnedOnUtc
            FROM progression.xp_entries
            WHERE trainer_id = @TrainerId
            ORDER BY earned_on_utc DESC
            LIMIT @RecentCount;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.TrainerId, RecentCount });

        // A trainer who never earned XP is at level 1 with none.
        ProgressRow row = await results.ReadSingleOrDefaultAsync<ProgressRow>() ?? new ProgressRow(0, 0, 0, 0, 0, 0, 0, 0);
        long totalXp = row.TotalXp;
        IEnumerable<EntryRow> recent = await results.ReadAsync<EntryRow>();

        int level = XpRules.LevelFor(totalXp);
        bool isMax = level >= XpRules.MaxLevel;

        return new TrainerProgressResponse(
            level,
            totalXp,
            totalXp - XpRules.TotalXpForLevel(level),
            isMax ? null : XpRules.XpToNextLevel(level),
            XpRules.MaxLevel,
            new TrainerStatsResponse(row.Catches, row.ShinyCatches, row.SpeciesDiscovered, row.Trades, row.Evolutions, row.Badges, row.Trophies),
            recent.Select(e => new XpEntryResponse(ToSnakeCase(e.Reason), e.Amount, e.EarnedOnUtc)).ToList());
    }

    private static string ToSnakeCase(string reason) =>
        string.Concat(reason.Select((c, i) => i > 0 && char.IsUpper(c) ? $"_{char.ToLowerInvariant(c)}" : char.ToLowerInvariant(c).ToString()));

    private sealed record EntryRow(string Reason, int Amount, DateTime EarnedOnUtc);

    private sealed record ProgressRow(long TotalXp, int Catches, int ShinyCatches, int SpeciesDiscovered, int Trades, int Evolutions, int Badges, int Trophies);
}
