using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.PublicApi;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Application.Trainers;

/// <summary>The trainer's progress in each game and across their games.</summary>
public sealed record GetTrainerAchievementsQuery(Guid TrainerId) : IQuery<TrainerAchievementsResponse>;

public sealed record TrainerAchievementsResponse(
    IReadOnlyCollection<GameAchievementsResponse> Games,
    CrossGameAchievementsResponse CrossGame);

public sealed record GameAchievementsResponse(
    Guid GameId,
    int Catches,
    int Evolutions,
    int ShinyCatches,
    int SpeciesRegistered,
    int Badges,
    int DexRegistered,
    int DexTotal,
    int TotalBadges);

public sealed record CrossGameAchievementsResponse(int GamesPlayed, int SameSpeciesCaught, int SameSpeciesEvolved);

internal static class TrainerAchievements
{
    public static AchievementProgress For(TrainerProgress progress, IEnumerable<GameFact> facts, IEnumerable<GameDexProgress> dexes) =>
        From(
            facts,
            progress.EarnedBadges.Select(b => b.GameId).ToList(),
            dexes,
            progress.Games.ToDictionary(g => g.GameId, g => g.TotalBadges));

    public static AchievementProgress From(
        IEnumerable<GameFact> facts,
        IReadOnlyCollection<Guid> badgeGameIds,
        IEnumerable<GameDexProgress> dexes,
        IReadOnlyDictionary<Guid, int> totalBadges)
    {
        var dexByGame = dexes.ToDictionary(d => d.GameId);

        var contexts = dexByGame.Keys
            .Concat(totalBadges.Keys)
            .Distinct()
            .Select(gameId => new GameContext(
                gameId,
                dexByGame.TryGetValue(gameId, out GameDexProgress? dex) ? dex.Registered : 0,
                dex?.Total ?? 0,
                totalBadges.GetValueOrDefault(gameId)))
            .ToList();

        return AchievementProgress.From(
            facts.Select(f => new AchievementFact(f.GameId, f.IsEvolution, f.PokemonId, f.SpeciesId, f.IsShiny)).ToList(),
            badgeGameIds,
            contexts);
    }
}

internal sealed class GetTrainerAchievementsQueryHandler(IDbConnectionFactory dbConnectionFactory, IPokedexApi pokedexApi)
    : IQueryHandler<GetTrainerAchievementsQuery, TrainerAchievementsResponse>
{
    public async Task<Result<TrainerAchievementsResponse>> Handle(GetTrainerAchievementsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<GameFact> facts = await pokedexApi.GetGameFactsAsync(request.TrainerId, cancellationToken);
        IReadOnlyCollection<GameDexProgress> dexes = await pokedexApi.GetGameDexProgressAsync(request.TrainerId, cancellationToken);

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT game_id FROM progression.trainer_badges WHERE trainer_id = @TrainerId;
            SELECT game_id AS GameId, total_badges AS TotalBadges FROM progression.trainer_games WHERE trainer_id = @TrainerId;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.TrainerId });

        var badgeGameIds = (await results.ReadAsync<Guid>()).ToList();
        var totalBadges = (await results.ReadAsync<GameBadgesRow>()).ToDictionary(g => g.GameId, g => g.TotalBadges);

        AchievementProgress progress = TrainerAchievements.From(facts, badgeGameIds, dexes, totalBadges);

        return new TrainerAchievementsResponse(
            progress.Games
                .Select(g => new GameAchievementsResponse(g.GameId, g.Catches, g.Evolutions, g.ShinyCatches, g.SpeciesRegistered, g.Badges, g.DexRegistered, g.DexTotal, g.TotalBadges))
                .ToList(),
            new CrossGameAchievementsResponse(
                progress.CrossGame.GamesPlayed,
                progress.CrossGame.SameSpeciesCaught,
                progress.CrossGame.SameSpeciesEvolved));
    }

    private sealed record GameBadgesRow(Guid GameId, int TotalBadges);
}
