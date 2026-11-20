using FluentValidation;
using PokeHub.Modules.Pokedex.Domain.Pokemon;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.CatchPokemon;

internal sealed class CatchPokemonCommandValidator : AbstractValidator<CatchPokemonCommand>
{
    public CatchPokemonCommandValidator()
    {
        RuleFor(c => c.SpeciesId).GreaterThan(0);

        RuleFor(c => c.Level).InclusiveBetween(OwnedPokemon.MinLevel, OwnedPokemon.MaxLevel);

        RuleFor(c => c.CatchKey).NotEmpty().MaximumLength(100);

        RuleFor(c => c.Nickname).MaximumLength(12);
    }
}
