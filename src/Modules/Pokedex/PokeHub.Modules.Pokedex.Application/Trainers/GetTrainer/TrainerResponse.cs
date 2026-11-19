namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;

/// <param name="StarterSpeciesId">Empty until the trainer chooses a starter.</param>
public sealed record TrainerResponse(Guid Id, string Name, int? StarterSpeciesId);
