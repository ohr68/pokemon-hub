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
    int Badges);

public sealed record CrossGameAchievementsResponse(int GamesPlayed, int SameSpeciesCaught, int SameSpeciesEvolved);

internal static class TrainerAchievements
{
    public static AchievementProgress For(TrainerProgress progress, IEnumerable<GameFact> facts) =>
        From(facts, progress.EarnedBadges.Select(b => b.GameId).ToList());

    public static AchievementProgress From(IEnumerable<GameFact> facts, IReadOnlyCollection<Guid> badgeGameIds) =>
        AchievementProgress.From(
            facts.Select(f => new AchievementFact(f.GameId, f.IsEvolution, f.PokemonId, f.SpeciesId, f.IsShiny)).ToList(),
            badgeGameIds);
}

internal sealed class GetTrainerAchievementsQueryHandler(IDbConnectionFactory dbConnectionFactory, IPokedexApi pokedexApi)
    : IQueryHandler<GetTrainerAchievementsQuery, TrainerAchievementsResponse>
{
    public async Task<Result<TrainerAchievementsResponse>> Handle(GetTrainerAchievementsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<GameFact> facts = await pokedexApi.GetGameFactsAsync(request.TrainerId, cancellationToken);

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql = "SELECT game_id FROM progression.trainer_badges WHERE trainer_id = @TrainerId";

        var badgeGameIds = (await connection.QueryAsync<Guid>(sql, new { request.TrainerId })).ToList();

        AchievementProgress progress = TrainerAchievements.From(facts, badgeGameIds);

        return new TrainerAchievementsResponse(
            progress.Games
                .Select(g => new GameAchievementsResponse(g.GameId, g.Catches, g.Evolutions, g.ShinyCatches, g.SpeciesRegistered, g.Badges))
                .ToList(),
            new CrossGameAchievementsResponse(
                progress.CrossGame.GamesPlayed,
                progress.CrossGame.SameSpeciesCaught,
                progress.CrossGame.SameSpeciesEvolved));
    }
}
