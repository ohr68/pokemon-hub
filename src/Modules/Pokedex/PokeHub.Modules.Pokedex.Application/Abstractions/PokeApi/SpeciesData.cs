namespace PokeHub.Modules.Pokedex.Application.Abstractions.PokeApi;

/// <param name="TradeEvolutionSpeciesId">The species it becomes when traded without a held item, if any.</param>
public sealed record SpeciesData(
    int Id,
    string Name,
    int Generation,
    string PrimaryType,
    string? SecondaryType,
    int? TradeEvolutionSpeciesId);
