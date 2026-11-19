using FluentValidation;

namespace PokeHub.Modules.Trades.Application.Trades.ProposeTrade;

internal sealed class ProposeTradeCommandValidator : AbstractValidator<ProposeTradeCommand>
{
    public ProposeTradeCommandValidator()
    {
        RuleFor(c => c.RecipientId).NotEmpty();

        RuleFor(c => c.OfferedPokemonId).NotEmpty();

        RuleFor(c => c.RequestedPokemonId).NotEmpty().NotEqual(c => c.OfferedPokemonId);
    }
}
