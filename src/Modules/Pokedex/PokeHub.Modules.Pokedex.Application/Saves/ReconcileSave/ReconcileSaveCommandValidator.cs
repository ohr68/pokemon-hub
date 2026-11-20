using FluentValidation;

namespace PokeHub.Modules.Pokedex.Application.Saves.ReconcileSave;

internal sealed class ReconcileSaveCommandValidator : AbstractValidator<ReconcileSaveCommand>
{
    public ReconcileSaveCommandValidator()
    {
        // An empty list would remove every Pokémon the game ever caught: far more likely a
        // save that failed to read than a real, empty game.
        RuleFor(c => c.PresentCatchKeys).NotEmpty();
    }
}
