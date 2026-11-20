namespace PokeHub.Modules.Games.Presentation.Play.Contracts;

internal sealed record ReconcileSaveRequest(IReadOnlyCollection<string> PresentCatchKeys, bool DryRun);
