using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Trades.Application.Trades.GetMyTrades;

internal sealed class GetMyTradesQueryHandler(IDbConnectionFactory dbConnectionFactory, IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetMyTradesQuery, IReadOnlyCollection<TradeResponse>>
{
    public async Task<Result<IReadOnlyCollection<TradeResponse>>> Handle(
        GetMyTradesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // Pending offers past their expiry are shown as expired even before anyone touches them.
        const string sql =
            """
            SELECT
               id AS Id,
               CASE WHEN proposer_id = @TrainerId THEN 'Outgoing' ELSE 'Incoming' END AS Direction,
               CASE WHEN status = 'Pending' AND expires_on_utc <= @UtcNow THEN 'Expired' ELSE status END AS Status,
               CASE WHEN proposer_id = @TrainerId THEN recipient_id ELSE proposer_id END AS OtherTrainerId,
               CASE WHEN proposer_id = @TrainerId THEN recipient_name ELSE proposer_name END AS OtherTrainerName,
               created_on_utc AS CreatedOnUtc,
               expires_on_utc AS ExpiresOnUtc,
               resolved_on_utc AS ResolvedOnUtc,
               failure_reason AS FailureReason,
               offered_pokemon_id AS PokemonId,
               offered_species_id AS SpeciesId,
               offered_species_name AS SpeciesName,
               offered_nickname AS Nickname,
               offered_level AS Level,
               offered_is_shiny AS IsShiny,
               requested_pokemon_id AS PokemonId,
               requested_species_id AS SpeciesId,
               requested_species_name AS SpeciesName,
               requested_nickname AS Nickname,
               requested_level AS Level,
               requested_is_shiny AS IsShiny
            FROM trades.trades
            WHERE proposer_id = @TrainerId OR recipient_id = @TrainerId
            ORDER BY created_on_utc DESC
            """;

        IEnumerable<TradeResponse> trades = await connection.QueryAsync<TradeRow, TradedPokemonResponse, TradedPokemonResponse, TradeResponse>(
            sql,
            (row, offered, requested) => new TradeResponse(
                row.Id,
                row.Direction,
                row.Status,
                row.OtherTrainerId,
                row.OtherTrainerName,
                offered,
                requested,
                row.CreatedOnUtc,
                row.ExpiresOnUtc,
                row.ResolvedOnUtc,
                row.FailureReason),
            new { request.TrainerId, dateTimeProvider.UtcNow },
            splitOn: "PokemonId,PokemonId");

        return trades.ToList();
    }

    private sealed record TradeRow(
        Guid Id,
        string Direction,
        string Status,
        Guid OtherTrainerId,
        string OtherTrainerName,
        DateTime CreatedOnUtc,
        DateTime ExpiresOnUtc,
        DateTime? ResolvedOnUtc,
        string? FailureReason);
}
