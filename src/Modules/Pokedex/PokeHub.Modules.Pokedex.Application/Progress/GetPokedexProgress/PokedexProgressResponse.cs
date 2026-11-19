namespace PokeHub.Modules.Pokedex.Application.Progress.GetPokedexProgress;

/// <param name="CaughtSpeciesIds">Every species the trainer has ever owned, even if since released.</param>
public sealed record PokedexProgressResponse(
    int Caught,
    int Total,
    IReadOnlyCollection<int> CaughtSpeciesIds);
