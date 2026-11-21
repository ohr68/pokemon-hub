using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Economy.Application.Wallets;

public sealed record GetWalletQuery(Guid TrainerId) : IQuery<WalletResponse>;

public sealed record WalletResponse(long Balance, IReadOnlyCollection<CoinEntryResponse> Recent);

/// <param name="Reason">caught_shiny, caught_rare, earned_badge, earned_trophy, leveled_up, purchase or refund.</param>
/// <param name="Amount">Positive when earned, negative when spent.</param>
public sealed record CoinEntryResponse(string Reason, int Amount, DateTime OccurredOnUtc);

internal sealed class GetWalletQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetWalletQuery, WalletResponse>
{
    public async Task<Result<WalletResponse>> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT balance FROM economy.wallets WHERE trainer_id = @TrainerId;

            SELECT reason AS Reason, amount AS Amount, occurred_on_utc AS OccurredOnUtc
            FROM economy.coin_entries WHERE trainer_id = @TrainerId
            ORDER BY occurred_on_utc DESC LIMIT 10;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.TrainerId });

        long balance = await results.ReadSingleOrDefaultAsync<long?>() ?? 0;
        IEnumerable<EntryRow> recent = await results.ReadAsync<EntryRow>();

        return new WalletResponse(balance, recent.Select(e => new CoinEntryResponse(Snake(e.Reason), e.Amount, e.OccurredOnUtc)).ToList());
    }

    internal static string Snake(string value) =>
        string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? $"_{char.ToLowerInvariant(c)}" : char.ToLowerInvariant(c).ToString()));

    private sealed record EntryRow(string Reason, int Amount, DateTime OccurredOnUtc);
}
