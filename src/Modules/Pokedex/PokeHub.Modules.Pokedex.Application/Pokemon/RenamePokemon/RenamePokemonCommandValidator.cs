using FluentValidation;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.RenamePokemon;

internal sealed class RenamePokemonCommandValidator : AbstractValidator<RenamePokemonCommand>
{
    // The main-series games allow up to 12 characters.
    public const int MaxNicknameLength = 12;

    public RenamePokemonCommandValidator()
    {
        RuleFor(c => c.Nickname).MaximumLength(MaxNicknameLength);
    }
}
