using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.PublicApi;
using PokeHub.Modules.Pokedex.Domain.Pokemon;

namespace PokeHub.Modules.Pokedex.Application.Saves.GameSpecies;

/// <summary>
/// A Pokémon can only enter a game that has its species.
/// </summary>
internal static class GameSpeciesRule
{
    public static async Task<Result> CheckAsync(
        IGameSpeciesLimits limits,
        Guid gameId,
        string gameName,
        int speciesId,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<Guid, int> maxSpeciesIds = await limits.GetMaxSpeciesIdsAsync([gameId], cancellationToken);

        return maxSpeciesIds.TryGetValue(gameId, out int maxSpeciesId) && speciesId > maxSpeciesId
            ? Result.Failure(PokemonErrors.SpeciesNotInGame(speciesId, gameName))
            : Result.Success();
    }
}
