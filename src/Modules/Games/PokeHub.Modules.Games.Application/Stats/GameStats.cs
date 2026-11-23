using System.Data.Common;
using System.Text.Json;
using Dapper;
using FluentValidation;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.Stats;
using PokeHub.Modules.Games.IntegrationEvents;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.Stats;

/// <param name="Counters">The game's own counters by name, such as steps or totalBattles.</param>
public sealed record ReportGameStatsCommand(
    string? ClientId,
    Guid TrainerId,
    long PlayTimeSeconds,
    long Money,
    int PokedexSeen,
    int PokedexOwned,
    IReadOnlyDictionary<string, long> Counters) : ICommand;

public sealed record ReportBattleDataCommand(
    string? ClientId,
    Guid TrainerId,
    IReadOnlyCollection<PokemonBattleReport> Pokemon) : ICommand;

public sealed record GetMyGameStatsQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<GameStatsResponse>>;

public sealed record GameStatsResponse(
    Guid GameId,
    string GameName,
    long PlayTimeSeconds,
    long Money,
    int PokedexSeen,
    int PokedexOwned,
    IReadOnlyDictionary<string, long> Counters,
    DateTime UpdatedOnUtc);

internal sealed class ReportGameStatsCommandValidator : AbstractValidator<ReportGameStatsCommand>
{
    private const int MaxCounters = 64;
    private const int MaxNameLength = 50;

    public ReportGameStatsCommandValidator()
    {
        RuleFor(c => c.PlayTimeSeconds).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Money).GreaterThanOrEqualTo(0);
        RuleFor(c => c.PokedexSeen).InclusiveBetween(0, 2000);
        RuleFor(c => c.PokedexOwned).InclusiveBetween(0, 2000);
        RuleFor(c => c.Counters).NotNull().Must(c => c.Count <= MaxCounters);
        RuleFor(c => c.Counters)
            .Must(c => c.All(pair => pair.Key.Length is > 0 and <= MaxNameLength && pair.Value >= 0))
            .WithMessage("Counters need a short name and a value of zero or more.");
    }
}

internal sealed class ReportGameStatsCommandHandler(
    IGameRepository gameRepository,
    IPlayerGameStatsRepository statsRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReportGameStatsCommand>
{
    public async Task<Result> Handle(ReportGameStatsCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure(game.Error);
        }

        PlayerGameStats? stats = await statsRepository.GetAsync(request.TrainerId, game.Value.Id, cancellationToken);

        if (stats is null)
        {
            stats = PlayerGameStats.Create(request.TrainerId, game.Value.Id);
            statsRepository.Insert(stats);
        }

        bool countersChanged = !SameCounters(stats.Counters, request.Counters);

        stats.Report(
            request.PlayTimeSeconds,
            request.Money,
            request.PokedexSeen,
            request.PokedexOwned,
            JsonSerializer.Serialize(request.Counters),
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (countersChanged)
        {
            await eventBus.PublishAsync(
                new GameStatsReportedIntegrationEvent(
                    Guid.CreateVersion7(),
                    dateTimeProvider.UtcNow,
                    request.TrainerId,
                    game.Value.Id),
                cancellationToken);
        }

        return Result.Success();
    }

    private static bool SameCounters(string stored, IReadOnlyDictionary<string, long> reported)
    {
        Dictionary<string, long> previous = JsonSerializer.Deserialize<Dictionary<string, long>>(stored) ?? [];

        return previous.Count == reported.Count
            && reported.All(pair => previous.TryGetValue(pair.Key, out long value) && value == pair.Value);
    }
}

internal sealed class ReportBattleDataCommandHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : ICommandHandler<ReportBattleDataCommand>
{
    public async Task<Result> Handle(ReportBattleDataCommand request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);

        if (game.IsFailure)
        {
            return Result.Failure(game.Error);
        }

        return await pokedexApi.UpdateBattleDataAsync(request.TrainerId, game.Value.Id, request.Pokemon, cancellationToken);
    }
}

internal sealed class GetMyGameStatsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMyGameStatsQuery, IReadOnlyCollection<GameStatsResponse>>
{
    public async Task<Result<IReadOnlyCollection<GameStatsResponse>>> Handle(GetMyGameStatsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT s.game_id AS GameId, g.name AS GameName, s.play_time_seconds AS PlayTimeSeconds, s.money AS Money,
                   s.pokedex_seen AS PokedexSeen, s.pokedex_owned AS PokedexOwned, s.counters::text AS Counters,
                   s.updated_on_utc AS UpdatedOnUtc
            FROM games.player_game_stats s
            JOIN games.games g ON g.id = s.game_id
            WHERE s.trainer_id = @TrainerId
            ORDER BY s.updated_on_utc DESC
            """;

        IEnumerable<StatsRow> rows = await connection.QueryAsync<StatsRow>(sql, new { request.TrainerId });

        return rows.Select(r => new GameStatsResponse(
            r.GameId,
            r.GameName,
            r.PlayTimeSeconds,
            r.Money,
            r.PokedexSeen,
            r.PokedexOwned,
            JsonSerializer.Deserialize<Dictionary<string, long>>(r.Counters) ?? [],
            r.UpdatedOnUtc)).ToList();
    }

    private sealed record StatsRow(
        Guid GameId,
        string GameName,
        long PlayTimeSeconds,
        long Money,
        int PokedexSeen,
        int PokedexOwned,
        string Counters,
        DateTime UpdatedOnUtc);
}
