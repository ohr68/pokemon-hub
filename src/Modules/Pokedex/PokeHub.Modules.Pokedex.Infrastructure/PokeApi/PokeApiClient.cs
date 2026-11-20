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
/// Trade evolutions add about 50 more requests.
/// </summary>
internal sealed partial class PokeApiClient(HttpClient httpClient, ILogger<PokeApiClient> logger) : IPokeApiClient
{
    // Generation III games were the first after the national dex reached 386, and number
    // species 1 to 251 the same way; Hoenn species (252 to 386) have their own numbers there.
    private const int LastGen2Species = 251;
    private const int LastGen3Species = 386;

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
            Dictionary<int, int> gen3Indices = await GetGen3IndicesAsync(species, cancellationToken);

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
                    gen3Indices.TryGetValue(id, out int gen3Index) ? gen3Index : null));
            }

            return new SpeciesCatalog(
                catalog,
                growthRates
                    .Select(rate => new GrowthRateData(
                        rate.Name,
                        rate.Levels.OrderBy(l => l.Level).Select(l => l.Experience).ToArray()))
                    .ToList());
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                                              or System.Text.Json.JsonException)
        {
            LogCatalogFailed(exception);

            return Result.Failure<SpeciesCatalog>(SpeciesErrors.CatalogUnavailable);
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
    /// Each species' number inside Generation III games: the same as the national number up to
    /// #251, read from PokéAPI's FireRed game index for #252 to #386.
    /// </summary>
    private async Task<Dictionary<int, int>> GetGen3IndicesAsync(
        Dictionary<int, (string Name, int Generation)> species,
        CancellationToken cancellationToken)
    {
        var indices = species.Keys
            .Where(id => id <= LastGen2Species)
            .ToDictionary(id => id, id => id);

        using var throttle = new SemaphoreSlim(MaxParallelRequests);

        PokemonGameIndicesResponse[] hoenn = await Task.WhenAll(species.Keys
            .Where(id => id is > LastGen2Species and <= LastGen3Species)
            .Select(async id =>
            {
                await throttle.WaitAsync(cancellationToken);

                try
                {
                    return await GetAsync<PokemonGameIndicesResponse>($"pokemon/{id}", cancellationToken);
                }
                finally
                {
                    throttle.Release();
                }
            }));

        foreach (PokemonGameIndicesResponse pokemon in hoenn)
        {
            GameIndex? fireRed = pokemon.GameIndices.FirstOrDefault(g => g.Version.Name == "firered");

            if (fireRed is not null)
            {
                indices[pokemon.Id] = fireRed.Index;
            }
        }

        return indices;
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<T>(new Uri(path, UriKind.Relative), cancellationToken)
               ?? throw new HttpRequestException($"PokéAPI returned an empty response for {path}.");
    }

    [LoggerMessage(LogLevel.Warning, "Species {SpeciesId} ({SpeciesName}) has no type in PokéAPI and was skipped")]
    private partial void LogSpeciesWithoutType(int speciesId, string speciesName);

    [LoggerMessage(LogLevel.Error, "Loading the species catalog from PokéAPI failed")]
    private partial void LogCatalogFailed(Exception exception);
}
