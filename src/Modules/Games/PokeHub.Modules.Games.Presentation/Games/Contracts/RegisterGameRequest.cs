namespace PokeHub.Modules.Games.Presentation.Games.Contracts;

internal sealed record RegisterGameRequest(
    string Name,
    string Slug,
    string? Description,
    Uri Url,
    IReadOnlyCollection<string> RedirectUris,
    IReadOnlyCollection<string> WebOrigins);
