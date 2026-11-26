using System.Text.Json.Serialization;

namespace PokeHub.Modules.Pokedex.Infrastructure.PokeApi;

// Only the fields the catalog sync reads.

internal sealed record NamedResource(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] Uri Url)
{
    // PokéAPI urls end in the resource id: .../pokemon-species/25/
    public int Id => int.Parse(Url.Segments[^1].TrimEnd('/'), System.Globalization.CultureInfo.InvariantCulture);
}

internal sealed record ResourceList(
    [property: JsonPropertyName("results")] IReadOnlyList<NamedResource> Results);

internal sealed record VersionGroupResponse(
    [property: JsonPropertyName("pokedexes")] IReadOnlyList<NamedResource> Pokedexes);

internal sealed record PokedexResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("pokemon_entries")] IReadOnlyList<PokedexEntryResponse> PokemonEntries);

internal sealed record PokedexEntryResponse(
    [property: JsonPropertyName("entry_number")] int EntryNumber,
    [property: JsonPropertyName("pokemon_species")] NamedResource PokemonSpecies);

internal sealed record GenerationResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("pokemon_species")] IReadOnlyList<NamedResource> PokemonSpecies);

internal sealed record TypeResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("pokemon")] IReadOnlyList<TypeSlot> Pokemon);

internal sealed record TypeSlot(
    [property: JsonPropertyName("slot")] int Slot,
    [property: JsonPropertyName("pokemon")] NamedResource Pokemon);

internal sealed record EvolutionTriggerResponse(
    [property: JsonPropertyName("pokemon_species")] IReadOnlyList<NamedResource> PokemonSpecies);

internal sealed record SpeciesEvolutionResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("evolves_from_species")] NamedResource? EvolvesFromSpecies,
    [property: JsonPropertyName("evolution_chain")] ApiResource EvolutionChain);

internal sealed record ApiResource(
    [property: JsonPropertyName("url")] Uri Url);

internal sealed record EvolutionChainResponse(
    [property: JsonPropertyName("chain")] ChainLink Chain);

internal sealed record ChainLink(
    [property: JsonPropertyName("species")] NamedResource Species,
    [property: JsonPropertyName("evolution_details")] IReadOnlyList<EvolutionDetail> EvolutionDetails,
    [property: JsonPropertyName("evolves_to")] IReadOnlyList<ChainLink> EvolvesTo);

internal sealed record EvolutionDetail(
    [property: JsonPropertyName("trigger")] NamedResource Trigger,
    [property: JsonPropertyName("held_item")] NamedResource? HeldItem,
    [property: JsonPropertyName("trade_species")] NamedResource? TradeSpecies);

internal sealed record GrowthRateResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("levels")] IReadOnlyList<GrowthRateLevel> Levels,
    [property: JsonPropertyName("pokemon_species")] IReadOnlyList<NamedResource> PokemonSpecies);

internal sealed record GrowthRateLevel(
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("experience")] int Experience);

internal sealed record PokemonResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("game_indices")] IReadOnlyList<GameIndex> GameIndices,
    [property: JsonPropertyName("moves")] IReadOnlyList<PokemonMove> Moves);

internal sealed record PokemonMove(
    [property: JsonPropertyName("move")] NamedResource Move,
    [property: JsonPropertyName("version_group_details")] IReadOnlyList<MoveLearnDetail> VersionGroupDetails);

internal sealed record MoveLearnDetail(
    [property: JsonPropertyName("level_learned_at")] int LevelLearnedAt,
    [property: JsonPropertyName("move_learn_method")] NamedResource MoveLearnMethod,
    [property: JsonPropertyName("version_group")] NamedResource VersionGroup);

internal sealed record MoveResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("pp")] int? Pp);

internal sealed record ItemResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("game_indices")] IReadOnlyList<ItemGameIndex> GameIndices);

internal sealed record ItemGameIndex(
    [property: JsonPropertyName("game_index")] int Index,
    [property: JsonPropertyName("generation")] NamedResource Generation);

internal sealed record GameIndex(
    [property: JsonPropertyName("game_index")] int Index,
    [property: JsonPropertyName("version")] NamedResource Version);

internal sealed record SpeciesRarityResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("is_legendary")] bool IsLegendary,
    [property: JsonPropertyName("is_mythical")] bool IsMythical,
    [property: JsonPropertyName("evolves_from_species")] NamedResource? EvolvesFromSpecies);
