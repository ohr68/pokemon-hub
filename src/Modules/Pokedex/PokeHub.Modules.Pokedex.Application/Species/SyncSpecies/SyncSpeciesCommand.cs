using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Species.SyncSpecies;

/// <param name="OnlyIfEmpty">Skips the sync when the catalog already has species, used on startup.</param>
public sealed record SyncSpeciesCommand(bool OnlyIfEmpty = false) : ICommand<SpeciesSyncResponse>;
