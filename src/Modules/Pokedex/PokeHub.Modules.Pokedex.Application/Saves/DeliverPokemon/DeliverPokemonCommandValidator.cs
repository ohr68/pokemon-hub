using FluentValidation;

namespace PokeHub.Modules.Pokedex.Application.Saves.DeliverPokemon;

internal sealed class DeliverPokemonCommandValidator : AbstractValidator<DeliverPokemonCommand>
{
    public DeliverPokemonCommandValidator()
    {
        RuleFor(c => c.Deliveries).NotEmpty();

        RuleForEach(c => c.Deliveries).ChildRules(delivery =>
        {
            delivery.RuleFor(d => d.PokemonId).NotEmpty();
            delivery.RuleFor(d => d.CatchKey).NotEmpty().MaximumLength(100);
        });

        RuleFor(c => c.Deliveries)
            .Must(d => d.Select(x => x.PokemonId).Distinct().Count() == d.Count
                       && d.Select(x => x.CatchKey).Distinct(StringComparer.Ordinal).Count() == d.Count)
            .WithMessage("Each Pokémon and each catch key can only appear once.");
    }
}
