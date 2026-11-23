using FluentValidation;

namespace PokeHub.Modules.Pokedex.Application.Saves.CancelWithdrawal;

internal sealed class CancelWithdrawalCommandValidator : AbstractValidator<CancelWithdrawalCommand>
{
    public CancelWithdrawalCommandValidator()
    {
        RuleFor(c => c.CatchKeys).NotEmpty();
        RuleForEach(c => c.CatchKeys).NotEmpty().MaximumLength(100);
        RuleFor(c => c.PresentCatchKeys).NotEmpty();
    }
}
