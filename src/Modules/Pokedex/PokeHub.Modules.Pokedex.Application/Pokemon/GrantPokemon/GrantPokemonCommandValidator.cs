using FluentValidation;
using PokeHub.Modules.Pokedex.Domain.Pokemon;

namespace PokeHub.Modules.Pokedex.Application.Pokemon.GrantPokemon;

internal sealed class GrantPokemonCommandValidator : AbstractValidator<GrantPokemonCommand>
{
    public GrantPokemonCommandValidator()
    {
        RuleFor(c => c.TrainerId).NotEmpty();

        RuleFor(c => c.SpeciesId).GreaterThan(0);

        RuleFor(c => c.Level).InclusiveBetween(OwnedPokemon.MinLevel, OwnedPokemon.MaxLevel);
    }
}
