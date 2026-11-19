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
/// </summary>
internal sealed partial class PokeApiClient(HttpClient httpClient, ILogger<PokeApiClient> logger) : IPokeApiClient
{
    public async Task<Result<IReadOnlyCollection<SpeciesData>>> GetSpeciesCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<int, (string Name, int Generation)> species = await GetSpeciesByGenerationAsync(cancellationToken);
            Dictionary<int, string[]> types = await GetTypesBySpeciesAsync(species, cancellationToken);

            var catalog = new List<SpeciesData>(species.Count);

            foreach ((int id, (string name, int generation)) in species.OrderBy(s => s.Key))
            {
                if (!types.TryGetValue(id, out string[]? slots) || slots[0] is null)
                {
                    LogSpeciesWithoutType(id, name);
                    continue;
                }

                catalog.Add(new SpeciesData(id, name, generation, slots[0], slots[1]));
            }

            return catalog;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                                              or System.Text.Json.JsonException)
        {
            LogCatalogFailed(exception);

            return Result.Failure<IReadOnlyCollection<SpeciesData>>(SpeciesErrors.CatalogUnavailable);
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
