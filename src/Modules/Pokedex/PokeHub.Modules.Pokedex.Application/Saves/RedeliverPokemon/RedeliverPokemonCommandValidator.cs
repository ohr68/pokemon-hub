using FluentValidation;

namespace PokeHub.Modules.Pokedex.Application.Saves.RedeliverPokemon;

internal sealed class RedeliverPokemonCommandValidator : AbstractValidator<RedeliverPokemonCommand>
{
    public RedeliverPokemonCommandValidator()
    {
        RuleFor(c => c.PokemonIds).NotEmpty();

        // An empty list is more likely a save that failed to read than a real, empty game.
        RuleFor(c => c.PresentCatchKeys).NotEmpty();
    }
}
