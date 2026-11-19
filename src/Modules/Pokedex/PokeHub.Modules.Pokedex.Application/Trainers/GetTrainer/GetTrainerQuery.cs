using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;

public sealed record GetTrainerQuery(Guid TrainerId) : IQuery<TrainerResponse>;
