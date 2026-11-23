using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Infrastructure.PokeApi;

/// <summary>
/// Builds the species catalog from about 30 PokéAPI requests instead of one per species:
/// each generation lists its species, and each type lists the Pokémon that have it and in
/// which slot. A species' types are those of its default form, whose id equals the species id.
/// Trade evolutions add about 50 more requests, and the Generation III data (game numbers and
/// FireRed/LeafGreen learnsets) one per species up to #386 plus one per move they learn, and
/// legendary/mythical flags one per species.
/// </summary>
internal sealed partial class PokeApiClient(HttpClient httpClient, ILogger<PokeApiClient> logger) : IPokeApiClient
{
    // Generation III games were the first after the national dex reached 386, and number
    // species 1 to 251 the same way; Hoenn species (252 to 386) have their own numbers there.
    private const int LastGen2Species = 251;
    private const int LastGen3Species = 386;

    // PokéAPI's name for FireRed and LeafGreen, which share their learnsets.
    private const string FireRedLeafGreen = "firered-leafgreen";

    // Keeps PokéAPI from receiving more than this many requests at once.
    private const int MaxParallelRequests = 10;

    public async Task<Result<SpeciesCatalog>> GetSpeciesCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<int, (string Name, int Generation)> species = await GetSpeciesByGenerationAsync(cancellationToken);
            Dictionary<int, string[]> types = await GetTypesBySpeciesAsync(species, cancellationToken);
            Dictionary<int, int> tradeEvolutions = await GetTradeEvolutionsAsync(species, cancellationToken);
            GrowthRateResponse[] growthRates = await GetGrowthRatesAsync(cancellationToken);
            Gen3Data gen3 = await GetGen3DataAsync(species, cancellationToken);
            Dictionary<int, SpeciesRarityResponse> rarity = await GetRarityAsync(species, cancellationToken);
            IReadOnlyCollection<MoveData> moves = await GetMovesAsync(
                gen3.LevelUpMoves.Values.SelectMany(learnset => learnset.Select(m => m.MoveId)).ToHashSet(),
                cancellationToken);

            var growthRateBySpecies = growthRates
                .SelectMany(rate => rate.PokemonSpecies.Select(s => (s.Id, rate.Name)))
                .ToDictionary(s => s.Id, s => s.Name);

            var catalog = new List<SpeciesData>(species.Count);

            foreach ((int id, (string name, int generation)) in species.OrderBy(s => s.Key))
            {
                if (!types.TryGetValue(id, out string[]? slots) || slots[0] is null)
                {
                    LogSpeciesWithoutType(id, name);
                    continue;
                }

                catalog.Add(new SpeciesData(
                    id,
                    name,
                    generation,
                    slots[0],
                    slots[1],
                    tradeEvolutions.TryGetValue(id, out int evolvesInto) ? evolvesInto : null,
                    growthRateBySpecies.GetValueOrDefault(id),
                    gen3.Indices.TryGetValue(id, out int gen3Index) ? gen3Index : null,
                    gen3.LevelUpMoves.GetValueOrDefault(id) ?? [],
                    rarity.GetValueOrDefault(id)?.IsLegendary ?? false,
                    rarity.GetValueOrDefault(id)?.IsMythical ?? false));
            }

            return new SpeciesCatalog(
                catalog,
                growthRates
                    .Select(rate => new GrowthRateData(
                        rate.Name,
                        rate.Levels.OrderBy(l => l.Level).Select(l => l.Experience).ToArray()))
                    .ToList(),
                moves);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                                              or System.Text.Json.JsonException)
        {
            LogCatalogFailed(exception);

            return Result.Failure<SpeciesCatalog>(SpeciesErrors.CatalogUnavailable);
        }
    }

    public async Task<Result<IReadOnlyCollection<GameDexData>>> GetGameDexesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            VersionGroupResponse[] groups = await ThrottledAsync(
                Enumerable.Range(0, GameDexes.All.Count),
                index => GetAsync<VersionGroupResponse>($"version-group/{GameDexes.All[index].VersionGroup}", cancellationToken),
                cancellationToken);

            var names = groups.SelectMany(g => g.Pokedexes.Select(p => p.Name)).Distinct().ToList();

            PokedexResponse[] pokedexes = await ThrottledAsync(
                Enumerable.Range(0, names.Count),
                index => GetAsync<PokedexResponse>($"pokedex/{names[index]}", cancellationToken),
                cancellationToken);

            Dictionary<string, PokedexResponse> byName = pokedexes.ToDictionary(p => p.Name);

            // A game with several Pokédexes (regions, expansions) lists them one after another.
            return GameDexes.All
                .Select((game, index) => new GameDexData(
                    game.Slug,
                    groups[index].Pokedexes
                        .SelectMany(p => byName[p.Name].PokemonEntries.OrderBy(e => e.EntryNumber))
                        .Select(e => e.PokemonSpecies.Id)
                        .Distinct()
                        .ToList()))
                .ToList();
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                                              or System.Text.Json.JsonException)
        {
            LogGameDexesFailed(exception);

            return Result.Failure<IReadOnlyCollection<GameDexData>>(SpeciesErrors.CatalogUnavailable);
        }
    }

    private async Task<Dictionary<int, (string Name, int Generation)>> GetSpeciesByGenerationAsync(
        CancellationToken cancellationToken)
    {
        ResourceList generations = await GetAsync<ResourceList>("generation?limit=100", cancellationToken);

        GenerationResponse[] responses = await Task.WhenAll(generations.Results.Select(generation =>
            GetAsync<GenerationResponse>($"generation/{generation.Id}", cancellationToken)));

        return responses
            .SelectMany(generation => generation.PokemonSpecies.Select(s => (s.Id, s.Name, Generation: generation.Id)))
            .ToDictionary(s => s.Id, s => (s.Name, s.Generation));
    }

    private async Task<Dictionary<int, string[]>> GetTypesBySpeciesAsync(
        Dictionary<int, (string Name, int Generation)> species,
        CancellationToken cancellationToken)
    {
        ResourceList typeList = await GetAsync<ResourceList>("type?limit=100", cancellationToken);

        TypeResponse[] responses = await Task.WhenAll(typeList.Results.Select(type =>
            GetAsync<TypeResponse>($"type/{type.Name}", cancellationToken)));

        var types = new Dictionary<int, string[]>();

        foreach (TypeResponse type in responses)
        {
            foreach (TypeSlot slot in type.Pokemon.Where(p => species.ContainsKey(p.Pokemon.Id) && p.Slot is 1 or 2))
            {
                if (!types.TryGetValue(slot.Pokemon.Id, out string[]? slots))
                {
                    slots = new string[2];
                    types[slot.Pokemon.Id] = slots;
                }

                slots[slot.Slot - 1] = type.Name;
            }
        }

        return types;
    }

    /// <summary>
    /// Maps each species to what it becomes when traded with no held item and no specific
    /// partner (Kadabra to Alakazam, Machoke to Machamp, ...). PokéAPI lists every species
    /// reached by trading; each one's evolution chain says what the trade requires.
    /// </summary>
    private async Task<Dictionary<int, int>> GetTradeEvolutionsAsync(
        Dictionary<int, (string Name, int Generation)> species,
        CancellationToken cancellationToken)
    {
        EvolutionTriggerResponse trade = await GetAsync<EvolutionTriggerResponse>("evolution-trigger/trade", cancellationToken);

        (int From, int Into)?[] evolutions = await Task.WhenAll(trade.PokemonSpecies
            .Where(target => species.ContainsKey(target.Id))
            .Select(target => GetPlainTradeEvolutionAsync(target.Id, cancellationToken)));

        return evolutions
            .OfType<(int From, int Into)>()
            .Where(e => species.ContainsKey(e.From))
            .ToDictionary(e => e.From, e => e.Into);
    }

    private async Task<(int From, int Into)?> GetPlainTradeEvolutionAsync(int targetId, CancellationToken cancellationToken)
    {
        SpeciesEvolutionResponse target = await GetAsync<SpeciesEvolutionResponse>(
            $"pokemon-species/{targetId}",
            cancellationToken);

        if (target.EvolvesFromSpecies is null)
        {
            return null;
        }

        EvolutionChainResponse chain = await httpClient.GetFromJsonAsync<EvolutionChainResponse>(
            target.EvolutionChain.Url,
            cancellationToken) ?? throw new HttpRequestException("PokéAPI returned an empty evolution chain.");

        ChainLink? link = Find(chain.Chain, targetId);

        bool plainTrade = link?.EvolutionDetails.Any(detail =>
            detail.Trigger.Name == "trade" && detail.HeldItem is null && detail.TradeSpecies is null) ?? false;

        return plainTrade ? (target.EvolvesFromSpecies.Id, targetId) : null;

        static ChainLink? Find(ChainLink node, int speciesId) =>
            node.Species.Id == speciesId
                ? node
                : node.EvolvesTo.Select(next => Find(next, speciesId)).FirstOrDefault(found => found is not null);
    }

    private async Task<GrowthRateResponse[]> GetGrowthRatesAsync(CancellationToken cancellationToken)
    {
        ResourceList rates = await GetAsync<ResourceList>("growth-rate?limit=20", cancellationToken);

        return await Task.WhenAll(rates.Results.Select(rate =>
            GetAsync<GrowthRateResponse>($"growth-rate/{rate.Name}", cancellationToken)));
    }

    /// <summary>
    /// For species 1 to 386: the number inside Generation III games (the same as the national
    /// number up to #251, FireRed's game index after that) and the FireRed/LeafGreen level-up
    /// learnset.
    /// </summary>
    private async Task<Gen3Data> GetGen3DataAsync(
        Dictionary<int, (string Name, int Generation)> species,
        CancellationToken cancellationToken)
    {
        PokemonResponse[] pokemon = await ThrottledAsync(
            species.Keys.Where(id => id <= LastGen3Species),
            id => GetAsync<PokemonResponse>($"pokemon/{id}", cancellationToken),
            cancellationToken);

        var indices = new Dictionary<int, int>();
        var levelUpMoves = new Dictionary<int, IReadOnlyList<LevelUpMoveData>>();

        foreach (PokemonResponse p in pokemon)
        {
            GameIndex? fireRed = p.GameIndices.FirstOrDefault(g => g.Version.Name == "firered");

            if (p.Id <= LastGen2Species)
            {
                indices[p.Id] = p.Id;
            }
            else if (fireRed is not null)
            {
                indices[p.Id] = fireRed.Index;
            }

            // PokéAPI lists moves by move, not by level; the game learns them by level.
            levelUpMoves[p.Id] = p.Moves
                .SelectMany(move => move.VersionGroupDetails
                    .Where(d => d.VersionGroup.Name == FireRedLeafGreen && d.MoveLearnMethod.Name == "level-up")
                    .Select(d => new LevelUpMoveData(d.LevelLearnedAt, move.Move.Id)))
                .OrderBy(m => m.Level)
                .ToList();
        }

        return new Gen3Data(indices, levelUpMoves);
    }

    /// <summary>Which species are legendary or mythical: one request per species.</summary>
    private async Task<Dictionary<int, SpeciesRarityResponse>> GetRarityAsync(
        Dictionary<int, (string Name, int Generation)> species,
        CancellationToken cancellationToken)
    {
        SpeciesRarityResponse[] rarity = await ThrottledAsync(
            species.Keys,
            id => GetAsync<SpeciesRarityResponse>($"pokemon-species/{id}", cancellationToken),
            cancellationToken);

        return rarity.ToDictionary(r => r.Id);
    }

    private async Task<IReadOnlyCollection<MoveData>> GetMovesAsync(
        IReadOnlyCollection<int> moveIds,
        CancellationToken cancellationToken)
    {
        MoveResponse[] moves = await ThrottledAsync(
            moveIds,
            id => GetAsync<MoveResponse>($"move/{id}", cancellationToken),
            cancellationToken);

        return moves.Select(m => new MoveData(m.Id, m.Name, m.Pp ?? 0)).ToList();
    }

    private static async Task<TResult[]> ThrottledAsync<TResult>(
        IEnumerable<int> ids,
        Func<int, Task<TResult>> request,
        CancellationToken cancellationToken)
    {
        using var throttle = new SemaphoreSlim(MaxParallelRequests);

        return await Task.WhenAll(ids.Select(async id =>
        {
            await throttle.WaitAsync(cancellationToken);

            try
            {
                return await request(id);
            }
            finally
            {
                throttle.Release();
            }
        }));
    }

    private sealed record Gen3Data(
        Dictionary<int, int> Indices,
        Dictionary<int, IReadOnlyList<LevelUpMoveData>> LevelUpMoves);

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<T>(new Uri(path, UriKind.Relative), cancellationToken)
               ?? throw new HttpRequestException($"PokéAPI returned an empty response for {path}.");
    }

    [LoggerMessage(LogLevel.Warning, "Species {SpeciesId} ({SpeciesName}) has no type in PokéAPI and was skipped")]
    private partial void LogSpeciesWithoutType(int speciesId, string speciesName);

    [LoggerMessage(LogLevel.Error, "Loading the game Pokédexes from PokéAPI failed")]
    private partial void LogGameDexesFailed(Exception exception);

    [LoggerMessage(LogLevel.Error, "Loading the species catalog from PokéAPI failed")]
    private partial void LogCatalogFailed(Exception exception);
}
