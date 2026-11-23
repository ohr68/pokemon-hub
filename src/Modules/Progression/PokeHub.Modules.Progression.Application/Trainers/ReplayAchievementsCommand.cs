using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Domain.Trainers;
using PokeHub.Modules.Progression.IntegrationEvents;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <summary>
/// Publishes every trophy, badge and level trainers already reached as integration events
/// again, so a module that started later (Economy's coins) can catch up. Each event's id is
/// derived from what it's about, so replaying again is ignored by consumers.
/// </summary>
public sealed record ReplayAchievementsCommand : ICommand<ReplayAchievementsResponse>;

public sealed record ReplayAchievementsResponse(int Trophies, int Badges, int Levels);

internal sealed class ReplayAchievementsCommandHandler(IDbConnectionFactory dbConnectionFactory, IEventBus eventBus)
    : ICommandHandler<ReplayAchievementsCommand, ReplayAchievementsResponse>
{
    public async Task<Result<ReplayAchievementsResponse>> Handle(ReplayAchievementsCommand request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT trainer_id AS TrainerId, trophy_code AS Code, earned_on_utc AS OnUtc FROM progression.trainer_trophies;
            SELECT trainer_id AS TrainerId, game_id AS GameId, code AS Code, earned_on_utc AS OnUtc FROM progression.trainer_badges;
            SELECT trainer_id AS TrainerId, level AS Level, updated_on_utc AS OnUtc FROM progression.trainer_progress WHERE level > 1;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql);

        var trophies = (await results.ReadAsync<TrophyRow>()).ToList();
        var badges = (await results.ReadAsync<BadgeRow>()).ToList();
        var levels = (await results.ReadAsync<LevelRow>()).ToList();

        foreach (TrophyRow t in trophies)
        {
            if (TrophyCatalog.TierOf(t.Code) is not { } tier)
            {
                continue;
            }

            await eventBus.PublishAsync(
                new TrophyEarnedIntegrationEvent(
                    IdFor($"trophy:{t.TrainerId}:{t.Code}"),
                    t.OnUtc,
                    t.TrainerId,
                    t.Code,
                    tier.ToString().ToLowerInvariant(),
                    TrophyCatalog.IsCrossGame(t.Code)),
                cancellationToken);
        }

        foreach (BadgeRow b in badges)
        {
            await eventBus.PublishAsync(
                new BadgeEarnedIntegrationEvent(IdFor($"badge:{b.TrainerId}:{b.GameId}:{b.Code}"), b.OnUtc, b.TrainerId, b.GameId, b.Code),
                cancellationToken);
        }

        // One event from level 1 to the current level; consumers pay per level reached.
        foreach (LevelRow l in levels)
        {
            await eventBus.PublishAsync(
                new TrainerLeveledUpIntegrationEvent(IdFor($"level:{l.TrainerId}:{l.Level}"), l.OnUtc, l.TrainerId, 1, l.Level),
                cancellationToken);
        }

        return new ReplayAchievementsResponse(trophies.Count, badges.Count, levels.Count);
    }

    // The same text always gives the same id (the first 16 bytes of its SHA-256).
    private static Guid IdFor(string text) => new(SHA256.HashData(Encoding.UTF8.GetBytes(text)).AsSpan(0, 16));

    private sealed record TrophyRow(Guid TrainerId, string Code, DateTime OnUtc);

    private sealed record BadgeRow(Guid TrainerId, Guid GameId, string Code, DateTime OnUtc);

    private sealed record LevelRow(Guid TrainerId, int Level, DateTime OnUtc);
}
