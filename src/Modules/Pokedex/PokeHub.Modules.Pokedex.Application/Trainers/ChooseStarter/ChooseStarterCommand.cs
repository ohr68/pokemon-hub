using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Trainers.ChooseStarter;

/// <returns>The identifier of the starter Pokémon.</returns>
public sealed record ChooseStarterCommand(Guid TrainerId, int SpeciesId) : ICommand<Guid>;
