using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.PublicApi;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GetGameFacts;

/// <param name="TrainerId">Only this trainer; empty for all of them.</param>
public sealed record GetGameDexProgressQuery(Guid? TrainerId) : IQuery<IReadOnlyCollection<GameDexProgressResponse>>;

public sealed record GameDexProgressResponse(Guid TrainerId, Guid GameId, string GameName, int Registered, int Total);

// Pokémon sent to a game count here, unlike for catch goals: as with a trade in the real
// games, that's the only way to register starters not chosen and version exclusives.
internal sealed class GetGameDexProgressQueryHandler(IDbConnectionFactory dbConnectionFactory, IGameSpeciesLimits games)
    : IQueryHandler<GetGameDexProgressQuery, IReadOnlyCollection<GameDexProgressResponse>>
{
    public async Task<Result<IReadOnlyCollection<GameDexProgressResponse>>> Handle(
        GetGameDexProgressQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT trainer_id AS TrainerId, game_id AS GameId, game_name AS GameName
            FROM pokedex.saves
            WHERE @TrainerId::uuid IS NULL OR trainer_id = @TrainerId;

            SELECT DISTINCT s.trainer_id AS TrainerId, s.game_id AS GameId, h.species_id AS SpeciesId
            FROM pokedex.ownership_history h
            JOIN pokedex.saves s ON s.id = h.save_id
            WHERE h.change IN ('Obtained', 'EvolvedInGame', 'MovedToSave')
              AND (@TrainerId::uuid IS NULL OR s.trainer_id = @TrainerId);
            """;

        await using SqlMapper.GridReader results = await connection.QueryMultipleAsync(sql, new { request.TrainerId });

        var saves = (await results.ReadAsync<SaveRow>()).ToList();
        ILookup<(Guid TrainerId, Guid GameId), int> registered = (await results.ReadAsync<RegisteredRow>())
            .ToLookup(r => (r.TrainerId, r.GameId), r => r.SpeciesId);

        IReadOnlyDictionary<Guid, GameIdentity> identities = await games.GetIdentitiesAsync(
            saves.Select(s => s.GameId).Distinct().ToList(),
            cancellationToken);

        var dexSlugs = identities
            .Select(i => (GameId: i.Key, Dex: GameDexes.ForGame(i.Value.Slug)))
            .Where(i => i.Dex is not null)
            .ToDictionary(i => i.GameId, i => i.Dex!.Slug);

        const string entriesSql =
            """
            SELECT game_slug AS Slug, species_id AS SpeciesId
            FROM pokedex.game_dex_entries
            WHERE game_slug = ANY(@Slugs)
            """;

        var dexes = (await connection.QueryAsync<EntryRow>(entriesSql, new { Slugs = dexSlugs.Values.Distinct().ToArray() }))
            .GroupBy(e => e.Slug)
            .ToDictionary(g => g.Key, g => g.Select(e => e.SpeciesId).ToHashSet());

        return saves
            .Select(save =>
            {
                HashSet<int> dex = dexSlugs.TryGetValue(save.GameId, out string? slug) && dexes.TryGetValue(slug, out HashSet<int>? species)
                    ? species
                    : [];

                return new GameDexProgressResponse(
                    save.TrainerId,
                    save.GameId,
                    identities.TryGetValue(save.GameId, out GameIdentity? identity) ? identity.Name : save.GameName,
                    registered[(save.TrainerId, save.GameId)].Count(dex.Contains),
                    dex.Count);
            })
            .ToList();
    }

    private sealed record SaveRow(Guid TrainerId, Guid GameId, string GameName);

    private sealed record RegisteredRow(Guid TrainerId, Guid GameId, int SpeciesId);

    private sealed record EntryRow(string Slug, int SpeciesId);
}
