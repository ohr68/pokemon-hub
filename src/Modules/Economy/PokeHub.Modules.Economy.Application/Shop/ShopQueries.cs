using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.Application.Wallets;

namespace PokeHub.Modules.Economy.Application.Shop;

/// <param name="All">Admins see every offer, inactive and sold out ones too.</param>
public sealed record GetOffersQuery(bool All) : IQuery<IReadOnlyCollection<OfferResponse>>;

/// <param name="Stock">How many are left; empty when there is no limit.</param>
public sealed record OfferResponse(
    Guid Id,
    int SpeciesId,
    string SpeciesName,
    bool IsRare,
    int Level,
    bool IsShiny,
    int Price,
    int? Stock,
    bool IsActive);

public sealed record GetMyPurchasesQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<PurchaseResponse>>;

/// <param name="Status">paid, delivered or refunded.</param>
public sealed record PurchaseResponse(
    Guid Id,
    int SpeciesId,
    string SpeciesName,
    int Level,
    bool IsShiny,
    int Price,
    string Status,
    DateTime PurchasedOnUtc);

internal sealed class GetOffersQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetOffersQuery, IReadOnlyCollection<OfferResponse>>
{
    public async Task<Result<IReadOnlyCollection<OfferResponse>>> Handle(GetOffersQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT id AS Id, species_id AS SpeciesId, species_name AS SpeciesName, is_rare AS IsRare, level AS Level,
                   is_shiny AS IsShiny, price AS Price, stock AS Stock, is_active AS IsActive
            FROM economy.shop_offers
            WHERE @All OR (is_active AND (stock IS NULL OR stock > 0))
            ORDER BY price, species_id
            """;

        IEnumerable<OfferResponse> offers = await connection.QueryAsync<OfferResponse>(sql, new { request.All });

        return offers.ToList();
    }
}

internal sealed class GetMyPurchasesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMyPurchasesQuery, IReadOnlyCollection<PurchaseResponse>>
{
    public async Task<Result<IReadOnlyCollection<PurchaseResponse>>> Handle(GetMyPurchasesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT id AS Id, species_id AS SpeciesId, species_name AS SpeciesName, level AS Level, is_shiny AS IsShiny,
                   price AS Price, status AS Status, purchased_on_utc AS PurchasedOnUtc
            FROM economy.purchases WHERE trainer_id = @TrainerId
            ORDER BY purchased_on_utc DESC LIMIT 20
            """;

        IEnumerable<PurchaseResponse> purchases = await connection.QueryAsync<PurchaseResponse>(sql, new { request.TrainerId });

        return purchases.Select(p => p with { Status = GetWalletQueryHandler.Snake(p.Status) }).ToList();
    }
}
