using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Trainers;

public static class TrainerErrors
{
    public static Error NotFound(Guid trainerId)
    {
        return Error.NotFound("Trainers.NotFound", $"The trainer with the identifier {trainerId} was not found.");
    }

    public static Error InvalidStarter(int speciesId)
    {
        return Error.Problem(
            "Trainers.InvalidStarter",
            $"The species with the number {speciesId} is not one of the starters.");
    }

    public static readonly Error StarterAlreadyChosen = Error.Conflict(
        "Trainers.StarterAlreadyChosen",
        "The trainer has already chosen a starter.");
}
