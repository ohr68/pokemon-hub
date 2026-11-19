using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Trainers.CreateTrainer;

public sealed record CreateTrainerCommand(Guid TrainerId, string Name) : ICommand;
