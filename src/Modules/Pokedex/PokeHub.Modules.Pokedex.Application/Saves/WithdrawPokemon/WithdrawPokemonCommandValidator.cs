using FluentValidation;

namespace PokeHub.Modules.Pokedex.Application.Saves.WithdrawPokemon;

internal sealed class WithdrawPokemonCommandValidator : AbstractValidator<WithdrawPokemonCommand>
{
    // Generous: a Generation III Pokémon takes 80 bytes.
    private const int MaxDataSize = 1024;

    public WithdrawPokemonCommandValidator()
    {
        RuleFor(c => c.Withdrawals).NotEmpty();

        RuleForEach(c => c.Withdrawals).ChildRules(withdrawal =>
        {
            withdrawal.RuleFor(w => w.CatchKey).NotEmpty().MaximumLength(100);
            withdrawal.RuleFor(w => w.SpeciesId).GreaterThan(0);
            withdrawal.RuleFor(w => w.Nickname).MaximumLength(12);
            withdrawal.RuleFor(w => w.DataFormat).NotEmpty().MaximumLength(20);
            withdrawal.RuleFor(w => w.Data).NotEmpty().Must(d => d.Length <= MaxDataSize);
        });

        RuleFor(c => c.Withdrawals)
            .Must(w => w.Select(x => x.CatchKey).Distinct(StringComparer.Ordinal).Count() == w.Count)
            .WithMessage("Each catch key can only appear once.");
    }
}
