using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.EventBus;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.IntegrationEvents;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.ReplayHistory;

/// <summary>
/// Publishes the ownership history again as the integration events it would have produced
/// (Pokémon obtained, traded and evolved), with their original dates. Lets a module that
/// started later (Progression) catch up. Each event reuses its history record's id, and
/// consumers pay each thing once, so replaying again changes nothing.
/// </summary>
public sealed record ReplayHistoryCommand : ICommand<ReplayHistoryResponse>;

public sealed record ReplayHistoryResponse(int Obtained, int Traded, int Evolved);

internal sealed class ReplayHistoryCommandHandler(IDbConnectionFactory dbConnectionFactory, IEventBus eventBus)
    : ICommandHandler<ReplayHistoryCommand, ReplayHistoryResponse>
{
    public async Task<Result<ReplayHistoryResponse>> Handle(ReplayHistoryCommand request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // A species is new for the trainer when nothing of theirs was that species earlier.
        const string sql =
            """
            SELECT h.id AS Id, h.occurred_on_utc AS OccurredOnUtc, h.pokemon_id AS PokemonId, h.trainer_id AS TrainerId,
                   h.species_id AS SpeciesId, lower(p.origin) AS Origin, p.is_shiny AS IsShiny,
                   (s.is_legendary OR s.is_mythical) AS IsRare,
                   NOT EXISTS (
                       SELECT 1 FROM pokedex.ownership_history e
                       WHERE e.trainer_id = h.trainer_id AND e.species_id = h.species_id
                         AND e.occurred_on_utc < h.occurred_on_utc) AS IsNewSpecies
            FROM pokedex.ownership_history h
            JOIN pokedex.pokemon p ON p.id = h.pokemon_id
            JOIN pokedex.species s ON s.id = h.species_id
            WHERE h.change = 'Obtained'
            ORDER BY h.occurred_on_utc;

            SELECT r.id AS Id, r.occurred_on_utc AS OccurredOnUtc, r.pokemon_id AS PokemonId, r.trade_id AS TradeId,
                   a.trainer_id AS FromTrainerId, r.trainer_id AS ToTrainerId
            FROM pokedex.ownership_history r
            JOIN pokedex.ownership_history a
              ON a.pokemon_id = r.pokemon_id AND a.trade_id = r.trade_id AND a.change = 'TradedAway'
            WHERE r.change = 'ReceivedInTrade'
            ORDER BY r.occurred_on_utc;

            SELECT id AS Id, occurred_on_utc AS OccurredOnUtc, pokemon_id AS PokemonId, trainer_id AS TrainerId,
                   from_species_id AS FromSpeciesId, species_id AS ToSpeciesId
            FROM (
                SELECT h.*, lag(h.species_id) OVER (PARTITION BY h.pokemon_id ORDER BY h.occurred_on_utc, h.id) AS from_species_id
                FROM pokedex.ownership_history h) history
            WHERE change IN ('EvolvedByTrade', 'EvolvedInGame') AND from_species_id IS NOT NULL
            ORDER BY occurred_on_utc;
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql);

        var obtained = (await results.ReadAsync<ObtainedRow>()).ToList();
        var traded = (await results.ReadAsync<TradedRow>()).ToList();
        var evolved = (await results.ReadAsync<EvolvedRow>()).ToList();

        foreach (ObtainedRow o in obtained)
        {
            await eventBus.PublishAsync(
                new PokemonObtainedIntegrationEvent(o.Id, o.OccurredOnUtc, o.PokemonId, o.TrainerId, o.SpeciesId, o.Origin, o.IsShiny, o.IsNewSpecies, o.IsRare),
                cancellationToken);
        }

        foreach (TradedRow t in traded)
        {
            await eventBus.PublishAsync(
                new PokemonTradedIntegrationEvent(t.Id, t.OccurredOnUtc, t.PokemonId, t.TradeId, t.FromTrainerId, t.ToTrainerId),
                cancellationToken);
        }

        foreach (EvolvedRow v in evolved)
        {
            await eventBus.PublishAsync(
                new PokemonEvolvedIntegrationEvent(v.Id, v.OccurredOnUtc, v.PokemonId, v.TrainerId, v.FromSpeciesId, v.ToSpeciesId),
                cancellationToken);
        }

        return new ReplayHistoryResponse(obtained.Count, traded.Count, evolved.Count);
    }

    private sealed record ObtainedRow(Guid Id, DateTime OccurredOnUtc, Guid PokemonId, Guid TrainerId, int SpeciesId, string Origin, bool IsShiny, bool IsRare, bool IsNewSpecies);

    private sealed record TradedRow(Guid Id, DateTime OccurredOnUtc, Guid PokemonId, Guid TradeId, Guid FromTrainerId, Guid ToTrainerId);

    private sealed record EvolvedRow(Guid Id, DateTime OccurredOnUtc, Guid PokemonId, Guid TrainerId, int FromSpeciesId, int ToSpeciesId);
}
