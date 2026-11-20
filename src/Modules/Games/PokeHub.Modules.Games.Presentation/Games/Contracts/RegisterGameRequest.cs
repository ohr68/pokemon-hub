namespace PokeHub.Modules.Games.Presentation.Games.Contracts;

internal sealed record RegisterGameRequest(
    string Name,
    string Slug,
    Uri Url,
    IReadOnlyCollection<string> RedirectUris,
    IReadOnlyCollection<string> WebOrigins);
