using FluentValidation;

namespace PokeHub.Modules.Pokedex.Application.Trainers.CreateTrainer;

internal sealed class CreateTrainerCommandValidator : AbstractValidator<CreateTrainerCommand>
{
    public CreateTrainerCommandValidator()
    {
        RuleFor(c => c.TrainerId).NotEmpty();

        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
    }
}
