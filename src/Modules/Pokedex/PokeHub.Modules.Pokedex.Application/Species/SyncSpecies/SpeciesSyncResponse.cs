namespace PokeHub.Modules.Pokedex.Application.Species.SyncSpecies;

public sealed record SpeciesSyncResponse(int Added, int Updated, int Total, bool Skipped = false);
