using System.Data.Common;
using Dapper;
using PokeHub.Common.Application.Data;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Species.GetGameDexes;

public sealed record GetGameDexesQuery : IQuery<IReadOnlyCollection<GameDexResponse>>;

/// <param name="SpeciesCount">Pokémon in the game's own Pokédex.</param>
/// <param name="NationalCount">Pokémon in its National Pokédex; empty when it doesn't have one.</param>
public sealed record GameDexResponse(string Slug, string Name, int Generation, int SpeciesCount, int? NationalCount);

internal sealed class GetGameDexesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetGameDexesQuery, IReadOnlyCollection<GameDexResponse>>
{
    public async Task<Result<IReadOnlyCollection<GameDexResponse>>> Handle(
        GetGameDexesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT game_slug AS Slug, COUNT(*)::int AS Count
            FROM pokedex.game_dex_entries
            GROUP BY game_slug
            """;

        var counts = (await connection.QueryAsync<CountRow>(sql)).ToDictionary(c => c.Slug, c => c.Count);

        // A game whose list hasn't loaded yet can't be filtered by.
        return GameDexes.All
            .Where(g => counts.ContainsKey(g.Slug))
            .Select(g => new GameDexResponse(g.Slug, g.Name, g.Generation, counts[g.Slug], g.NationalMaxSpeciesId))
            .ToList();
    }

    private sealed record CountRow(string Slug, int Count);
}
