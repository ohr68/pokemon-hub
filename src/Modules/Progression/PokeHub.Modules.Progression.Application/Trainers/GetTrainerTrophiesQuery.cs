using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Progression.Domain.Trainers;

using PokeHub.Modules.Games.PublicApi;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <summary>Every trophy, earned or not, with how close the trainer is.</summary>
public sealed record GetTrainerTrophiesQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<TrophyResponse>>;

/// <param name="Tier">bronze, silver or gold.</param>
/// <param name="Progress">The trainer's count toward <paramref name="Target"/>, capped at it.</param>
/// <param name="GameId">The game a per-game trophy belongs to; empty for the others.</param>
/// <param name="IsCrossGame">It rewards what the trainer did across their games.</param>
/// <param name="IsSpecific">A hand-made goal, for one game or for the hub.</param>
public sealed record TrophyResponse(
    string Code,
    string Name,
    string Description,
    string Tier,
    int Progress,
    int Target,
    DateTime? EarnedOnUtc,
    Guid? GameId = null,
    string? GameName = null,
    bool IsCrossGame = false,
    bool IsSpecific = false);

internal sealed class GetTrainerTrophiesQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IPokedexApi pokedexApi,
    IPlayerGameCounters gameCounters)
    : IQueryHandler<GetTrainerTrophiesQuery, IReadOnlyCollection<TrophyResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrophyResponse>>> Handle(GetTrainerTrophiesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<GameFact> facts = await pokedexApi.GetGameFactsAsync(request.TrainerId, cancellationToken);
        IReadOnlyCollection<GameDexProgress> dexes = await pokedexApi.GetGameDexProgressAsync(request.TrainerId, cancellationToken);
        SpecificSources sources = await TrainerAchievements.LoadSourcesAsync(pokedexApi, gameCounters, request.TrainerId, cancellationToken);

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT starters_chosen AS StartersChosen, catches AS Catches, shiny_catches AS ShinyCatches,
                   species_discovered AS SpeciesDiscovered, trades AS Trades, evolutions AS Evolutions, level AS Level, badges AS Badges
            FROM progression.trainer_progress WHERE trainer_id = @TrainerId;

            SELECT trophy_code AS TrophyCode, earned_on_utc AS EarnedOnUtc
            FROM progression.trainer_trophies WHERE trainer_id = @TrainerId;

            SELECT game_id FROM progression.trainer_badges WHERE trainer_id = @TrainerId;

            SELECT game_id AS GameId, game_name AS GameName, total_badges AS TotalBadges
            FROM progression.trainer_games WHERE trainer_id = @TrainerId;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.TrainerId });

        StatsRow stats = await results.ReadSingleOrDefaultAsync<StatsRow>() ?? new StatsRow(0, 0, 0, 0, 0, 0, 1, 0);
        var earned = (await results.ReadAsync<EarnedRow>()).ToDictionary(e => e.TrophyCode, e => e.EarnedOnUtc);

        var badgeGameIds = (await results.ReadAsync<Guid>()).ToList();
        var reportedGames = (await results.ReadAsync<GameRow>()).ToList();

        AchievementProgress achievements = TrainerAchievements.From(
            facts,
            badgeGameIds,
            dexes,
            reportedGames.ToDictionary(g => g.GameId, g => g.TotalBadges));

        var gameNames = reportedGames.ToDictionary(g => g.GameId, g => g.GameName);

        foreach (GameDexProgress dex in dexes)
        {
            gameNames[dex.GameId] = dex.GameName;
        }

        SpecificProgress specific = TrainerAchievements.Specific(request.TrainerId, facts, achievements, sources);

        var perGame = achievements.Games
            .SelectMany(game => GameTrophies.All
                .Where(game.Offers)
                .Select(t => ForGame(t, game, ShortName(gameNames.GetValueOrDefault(game.GameId, "Jogo")), earned)))
            .ToList();

        return Trophies.All
            .Select(t => new TrophyResponse(
                t.Code,
                t.Name,
                t.Description,
                t.Tier.ToString().ToLowerInvariant(),
                Math.Min(stats.Value(t.Stat), t.Target),
                t.Target,
                earned.TryGetValue(t.Code, out DateTime on) ? on : null))
            .Concat(perGame)
            .Concat(CrossGameTrophies.All.Select(t => new TrophyResponse(
                t.Code,
                t.Name,
                t.Description,
                t.Tier.ToString().ToLowerInvariant(),
                Math.Min(achievements.CrossGame.Value(t.Stat), t.Target),
                t.Target,
                earned.TryGetValue(t.Code, out DateTime earnedOn) ? earnedOn : null,
                IsCrossGame: true)))
            .Concat(SpecificTrophies.All
                .Where(t => t.GameSlug is null || sources.GameIds.ContainsKey(t.GameSlug))
                .Select(t => ForSpecific(t, specific, sources.GameIds, gameNames, earned)))
            .ToList();
    }

    private static TrophyResponse ForGame(GameTrophy trophy, GameProgress game, string gameName, Dictionary<string, DateTime> earned)
    {
        string code = trophy.CodeFor(game.GameId);
        (int current, int target) = game.Measure(trophy);

        return new TrophyResponse(
            code,
            trophy.Name,
            $"{gameName}: {trophy.Description}",
            trophy.Tier.ToString().ToLowerInvariant(),
            Math.Min(current, target),
            target,
            earned.TryGetValue(code, out DateTime on) ? on : null,
            game.GameId,
            gameName);
    }

    private static TrophyResponse ForSpecific(
        SpecificTrophy trophy,
        SpecificProgress specific,
        IReadOnlyDictionary<string, Guid> gameIds,
        Dictionary<Guid, string> gameNames,
        Dictionary<string, DateTime> earned)
    {
        Guid? gameId = trophy.GameSlug is not null ? gameIds[trophy.GameSlug] : null;
        string? gameName = gameId is { } id && gameNames.TryGetValue(id, out string? name) ? ShortName(name) : null;

        return new TrophyResponse(
            trophy.Code,
            trophy.Name,
            trophy.Description,
            trophy.Tier.ToString().ToLowerInvariant(),
            Math.Min(specific.Value(trophy), trophy.Target),
            trophy.Target,
            earned.TryGetValue(trophy.Code, out DateTime on) ? on : null,
            gameId,
            gameName,
            IsSpecific: true);
    }

    private static string ShortName(string gameName) =>
        gameName.StartsWith("Pokémon ", StringComparison.Ordinal) ? gameName["Pokémon ".Length..] : gameName;

    private sealed record GameRow(Guid GameId, string GameName, int TotalBadges);

    private sealed record StatsRow(int StartersChosen, int Catches, int ShinyCatches, int SpeciesDiscovered, int Trades, int Evolutions, int Level, int Badges)
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
            TrainerStat.Badges => Badges,
            _ => 0,
        };
    }

    private sealed record EarnedRow(string TrophyCode, DateTime EarnedOnUtc);
}
