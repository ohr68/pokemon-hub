namespace PokeHub.Modules.Games.Presentation.SaveFiles.Contracts;

internal sealed class SaveFileRouteRequest
{
    // Not in the route when asking for the latest backup.
    public Guid? SaveFileId { get; init; }
}
