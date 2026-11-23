using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.PublicApi;
using PokeHub.Modules.Pokedex.PublicApi;
using PokeHub.Modules.Progression.Application.Abstractions.Data;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

public sealed record BadgeInfo(string Code, string Name);

/// <param name="Badges">Every badge the game says the trainer has earned so far.</param>
public sealed record RecordBadgesCommand(Guid TrainerId, Guid GameId, string GameName, int TotalBadges, IReadOnlyCollection<BadgeInfo> Badges) : ICommand;

internal sealed class RecordBadgesCommandHandler(
    ITrainerProgressRepository repository,
    IPokedexApi pokedexApi,
    IPlayerGameCounters gameCounters,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RecordBadgesCommand>
{
    public async Task<Result> Handle(RecordBadgesCommand request, CancellationToken cancellationToken)
    {
        DateTime utcNow = dateTimeProvider.UtcNow;
        TrainerProgress? progress = await repository.GetAsync(request.TrainerId, cancellationToken);

        if (progress is null)
        {
            progress = TrainerProgress.Start(request.TrainerId, utcNow);
            repository.Insert(progress);
        }

        progress.RecordBadges(request.GameId, request.GameName, request.TotalBadges, request.Badges.Select(b => (b.Code, b.Name)), utcNow);
        progress.EarnTrophies(utcNow);

        IReadOnlyCollection<GameFact> facts = await pokedexApi.GetGameFactsAsync(request.TrainerId, cancellationToken);
        IReadOnlyCollection<GameDexProgress> dexes = await pokedexApi.GetGameDexProgressAsync(request.TrainerId, cancellationToken);
        AchievementProgress achievements = TrainerAchievements.For(progress, facts, dexes);
        progress.EarnAchievements(achievements, utcNow);

        SpecificSources sources = await TrainerAchievements.LoadSourcesAsync(pokedexApi, gameCounters, request.TrainerId, cancellationToken);
        progress.EarnSpecific(TrainerAchievements.Specific(request.TrainerId, facts, achievements, sources), utcNow);

        repository.InsertEntries(progress.NewEntries);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

/// <summary>The trainer's badges, per game that reported them.</summary>
public sealed record GetTrainerBadgesQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<GameBadgesResponse>>;

public sealed record GameBadgesResponse(Guid GameId, string GameName, int TotalBadges, IReadOnlyCollection<EarnedBadgeResponse> Badges);

public sealed record EarnedBadgeResponse(string Code, string Name, DateTime EarnedOnUtc);

internal sealed class GetTrainerBadgesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTrainerBadgesQuery, IReadOnlyCollection<GameBadgesResponse>>
{
    public async Task<Result<IReadOnlyCollection<GameBadgesResponse>>> Handle(GetTrainerBadgesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT game_id AS GameId, game_name AS GameName, total_badges AS TotalBadges
            FROM progression.trainer_games WHERE trainer_id = @TrainerId ORDER BY game_name;

            SELECT game_id AS GameId, code AS Code, name AS Name, earned_on_utc AS EarnedOnUtc
            FROM progression.trainer_badges WHERE trainer_id = @TrainerId ORDER BY earned_on_utc;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.TrainerId });

        var games = (await results.ReadAsync<GameRow>()).ToList();
        ILookup<Guid, BadgeRow> badges = (await results.ReadAsync<BadgeRow>()).ToLookup(b => b.GameId);

        return games
            .Select(g => new GameBadgesResponse(
                g.GameId,
                g.GameName,
                g.TotalBadges,
                badges[g.GameId].Select(b => new EarnedBadgeResponse(b.Code, b.Name, b.EarnedOnUtc)).ToList()))
            .ToList();
    }

    private sealed record GameRow(Guid GameId, string GameName, int TotalBadges);

    private sealed record BadgeRow(Guid GameId, string Code, string Name, DateTime EarnedOnUtc);
}
