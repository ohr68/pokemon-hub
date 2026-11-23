using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

/// <param name="Search">Part of the name, or the exact Pokédex number.</param>
/// <param name="Game">Only this game's Pokédex, in the game's order.</param>
/// <param name="National">With a game: its National Pokédex instead of its own, when it has one.</param>
public sealed record GetSpeciesQuery(
    string? Search,
    string? Type,
    int? Generation,
    int Page,
    int PageSize,
    string? Game = null,
    bool National = false) : IQuery<SpeciesPageResponse>;
