namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;

/// <param name="Code">The public code other trainers use to find this one.</param>
/// <param name="StarterSpeciesId">Empty until the trainer chooses a starter.</param>
public sealed record TrainerResponse(Guid Id, string Name, string Code, int? StarterSpeciesId);
